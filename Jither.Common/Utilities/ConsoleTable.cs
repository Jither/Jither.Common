using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jither.Utilities
{
    [Flags]
    public enum ConsoleColumnFormat : uint
    {
        None = 0,
        RightAligned = 1,
        AutoSize = 2,
        Wrap = 4,
    }

    internal class ConsoleTableColumn
    {
        public int Width { get; }
        public ConsoleColumnFormat Format { get; }
        public int CalculatedWidth { get; set; }

        public ConsoleTableColumn(int width, ConsoleColumnFormat format)
        {
            Width = width;
            Format = format;
        }

        public void Initialize()
        {
            CalculatedWidth = Width;
        }
    }

    internal class ConsoleTableRow
    {
        public IReadOnlyList<object> Cells { get; }
        public List<string> CellValues { get; } = new List<string>();

        public ConsoleTableRow(IReadOnlyList<object> cells)
        {
            Cells = cells;
            CellValues = cells.Select(c => c?.ToString() ?? String.Empty).ToList();
        }
    }

    internal class WrappingRemainder
    {
        public ConsoleTableColumn Column { get; }
        public List<string> Lines { get; }

        public WrappingRemainder(ConsoleTableColumn column, List<string> lines)
        {
            Column = column;
            Lines = lines;
        }
    }

    public class ConsoleTable
    {
        private readonly List<ConsoleTableColumn> columns = new List<ConsoleTableColumn>();
        private readonly List<ConsoleTableRow> rows = new List<ConsoleTableRow>();
        private readonly int columnSpacing;

        public ConsoleTable(int columnSpacing = 2)
        {
            this.columnSpacing = columnSpacing;
        }

        public void AddColumn(int width, ConsoleColumnFormat format = ConsoleColumnFormat.None)
        {
            if (format.HasFlag(ConsoleColumnFormat.AutoSize))
            {
                if (columns.Any(c => c.Format.HasFlag(ConsoleColumnFormat.AutoSize)))
                {
                    throw new ArgumentException("Only one column with auto sizing is allowed for now");
                }
            }
            columns.Add(new ConsoleTableColumn(width, format));
        }

        public void AddRow(params object[] cells)
        {
            if (columns.Count != cells.Length)
            {
                throw new ArgumentException($"Number of cells ({cells.Length}) doesn't match table's columns ({columns.Count})", nameof(cells));
            }

            rows.Add(new ConsoleTableRow(cells));
        }

        public string ToString(int width)
        {
            CalculateRequiredWidths(width - (columns.Count - 1) * columnSpacing);

            StringBuilder builder = new StringBuilder();

            string columnSpace = new String(' ', columnSpacing);

            List<WrappingRemainder> wrappingRemainders = new List<WrappingRemainder>();

            foreach (var row in rows)
            {
                int cellIndex = 0;
                foreach (var column in columns)
                {
                    string text = row.CellValues[cellIndex];

                    if (text.Length > column.CalculatedWidth && (column.Format.HasFlag(ConsoleColumnFormat.AutoSize) || column.Format.HasFlag(ConsoleColumnFormat.Wrap)))
                    {
                        var lines = text.Wrap(column.CalculatedWidth);
                        text = MakeCellLine(column, lines[0]);
                        builder.Append(text);
                        wrappingRemainders.Add(new WrappingRemainder(column, lines.Skip(1).ToList()));
                    }
                    else
                    {
                        text = MakeCellLine(column, text);
                        builder.Append(text);
                    }

                    // Add space between columns
                    if (cellIndex < columns.Count - 1)
                    {
                        builder.Append(columnSpace);
                    }
                    cellIndex++;
                }
                builder.AppendLine();

                // Add wrapped lines
                while (wrappingRemainders.Count > 0)
                {
                    cellIndex = 0;
                    foreach (var column in columns)
                    {
                        var remainder = wrappingRemainders.Find(w => w.Column == column);
                        string line = String.Empty;
                        if (remainder != null)
                        {
                            line = remainder.Lines[0];
                            remainder.Lines.RemoveAt(0);
                            if (remainder.Lines.Count == 0)
                            {
                                wrappingRemainders.Remove(remainder);
                            }
                        }
                        line = MakeCellLine(column, line);
                        builder.Append(line);
                        if (cellIndex < columns.Count - 1)
                        {
                            builder.Append(columnSpace);
                        }
                        cellIndex++;
                    }
                    builder.AppendLine();
                }
            }
            return builder.ToString();
        }

        private static string MakeCellLine(ConsoleTableColumn column, string text)
        {
            if (column.Format.HasFlag(ConsoleColumnFormat.RightAligned))
            {
                text = text.PadLeft(column.CalculatedWidth, ' ');
            }
            else
            {
                text = text.PadRight(column.CalculatedWidth, ' ');
            }

            return text;
        }

        private void CalculateRequiredWidths(int availableWidth)
        {
            foreach (var column in columns)
            {
                column.Initialize();
            }

            foreach (var row in rows)
            {
                int cellIndex = 0;
                foreach (var column in columns)
                {
                    string text = row.CellValues[cellIndex];
                    column.CalculatedWidth = column.Format.HasFlag(ConsoleColumnFormat.Wrap) ? column.CalculatedWidth : Math.Max(text.Length, column.CalculatedWidth);
                    cellIndex++;
                }
            }

            var autoSizingColumn = columns.SingleOrDefault(c => c.Format.HasFlag(ConsoleColumnFormat.AutoSize));
            if (autoSizingColumn != null)
            {
                // The width of the auto sizing column is the remainder after all columns get their share.
                // Obviously except the auto sizing column itself.
                // ... but at least the user specified width of the auto sizing column (this to avoid negative
                // width for narrow windows, although it will break the layout severely).
                autoSizingColumn.CalculatedWidth = Math.Max(
                    availableWidth - columns.Where(c => c != autoSizingColumn).Sum(c => c.CalculatedWidth), 
                    autoSizingColumn.Width
                );
            }
        }
    }
}
