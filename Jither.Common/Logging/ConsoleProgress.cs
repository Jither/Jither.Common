using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Jither.Logging;

public class ProgressSlot
{
    public int Index { get; }
    public int Position { get; }
    public int Length { get; }

    public ProgressSlot(int index, int position, int length)
    {
        Index = index;
        Position = position;
        Length = length;
    }
}

public class ConsoleProgress : IDisposable
{
    private readonly int positionX;
    private readonly int positionY;
    private readonly bool disabled;
    private readonly List<ProgressSlot> slots = new();
    private IList<object> values;
    private bool disposed;

    private static readonly int UPDATE_INTERVAL = 20;

    private static readonly Regex RX_SLOT = new(@"\{(?<index>\d+)\}");

    private readonly Stopwatch stopwatch;

    public ConsoleProgress(string template, params int[] slotLengths)
    {
        if (Console.IsOutputRedirected)
        {
            disabled = true;
            return;
        }
        positionX = Console.CursorLeft;
        positionY = Console.CursorTop;

        int slotPosition = 0;
        int lastIndex = 0;
        template = RX_SLOT.Replace(template, match =>
        {
            int index = Int32.Parse(match.Groups["index"].Value);
            int slotLength = slotLengths[index];
            slotPosition += match.Index - lastIndex;
            slots.Add(new ProgressSlot(index, slotPosition, slotLength));
            slotPosition += slotLength - match.Length;
            lastIndex = match.Index;
            return new String(' ', slotLength);
        });

        // Using stopwatch rather than e.g. a Timer in order to avoid running on a different thread.
        // The console output needs to be single threaded across the application to avoid e.g. cursor
        // movements interfering with other attempts to write to the console (e.g. from the logger).
        stopwatch = new Stopwatch();
        stopwatch.Start();
        Console.Write(template);
    }

    public void Write(params object[] values)
    {
        if (disabled)
        {
            return;
        }
        this.values = values;
        if (stopwatch.ElapsedMilliseconds > UPDATE_INTERVAL)
        {
            Update();
            stopwatch.Restart();
        }
    }

    private void Update()
    {
        if (disabled)
        {
            return;
        }
        int currentX = Console.CursorLeft;
        int currentY = Console.CursorTop;

        bool cursorWasVisible = !OperatingSystem.IsWindows() || Console.CursorVisible;

        Console.CursorVisible = false;

        for (int slotIndex = 0; slotIndex < slots.Count; slotIndex++)
        {
            var slot = slots[slotIndex];
            Console.SetCursorPosition(positionX + slot.Position, positionY);
            Console.Write(values[slot.Index].ToString().PadLeft(slot.Length));
        }

        Console.SetCursorPosition(currentX, currentY);
        Console.CursorVisible = cursorWasVisible;
    }

    public void Dispose()
    {
        if (!disposed)
        {
            Update();
            disposed = true;
        }
        GC.SuppressFinalize(this);
    }
}
