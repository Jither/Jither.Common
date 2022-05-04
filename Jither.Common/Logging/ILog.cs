using System;

namespace Jither.Logging;

public interface ILog
{
    void Log(string loggerName, LogLevel level, DateTime time, string message);
}
