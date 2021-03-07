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
            CellValues = cells.Select(c => c.ToString()).ToList();
        }
    }

    internal class WrappingRemainder
    {
        public ConsoleTableColumn Column { get; }
        public IEnumerable<string> Lines { get; }
        public int LeftMargin { get; }

        public WrappingRemainder(ConsoleTableColumn column, IEnumerable<string> lines, int leftMargin)
        {
            Column = column;
            Lines = lines;
            LeftMargin = leftMargin;
        }
    }

    public class ConsoleTable
    {
        private List<ConsoleTableColumn> columns = new List<ConsoleTableColumn>();
        private List<ConsoleTableRow> rows = new List<ConsoleTableRow>();
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

            WrappingRemainder wrappingRemainder = null;

            foreach (var row in rows)
            {
                int cellIndex = 0;
                int rowLength = 0;
                foreach (var column in columns)
                {
                    string text = row.CellValues[cellIndex];

                    if (column.Format.HasFlag(ConsoleColumnFormat.AutoSize) && text.Length > column.CalculatedWidth)
                    {
                        var lines = text.Wrap(column.CalculatedWidth);
                        text = MakeCellLine(column, lines[0]);
                        builder.Append(text);
                        wrappingRemainder = new WrappingRemainder(column, lines.Skip(1), rowLength);
                    }
                    else
                    {
                        text = MakeCellLine(column, text);
                        builder.Append(text);
                    }

                    rowLength += column.CalculatedWidth;

                    // Add space between columns
                    if (cellIndex < columns.Count - 1)
                    {
                        builder.Append(columnSpace);
                        rowLength += columnSpacing;
                    }
                    cellIndex++;
                }
                builder.AppendLine();

                if (wrappingRemainder != null)
                {
                    foreach (var line in wrappingRemainder.Lines)
                    {
                        var text = MakeCellLine(wrappingRemainder.Column, line);
                        builder.AppendLine(new String(' ', wrappingRemainder.LeftMargin) + text);
                    }
                    wrappingRemainder = null;
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
                    column.CalculatedWidth = Math.Max(text.Length, column.CalculatedWidth);
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
