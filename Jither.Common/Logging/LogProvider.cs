using System;
using System.Collections.Generic;

namespace Jither.Logging;

public interface ILogWithLifetime
{
    void Start();
    void End();
}

public static class LogProvider
{
    private static readonly List<ILog> logs = new();
    private static readonly Dictionary<string, Logger> loggersByName = new();

    public static LogLevel Level { get; set; } = LogLevel.Info;

    public static Logger Default { get; } = Get();

    public static Logger Get(string name = "")
    {
        if (!loggersByName.TryGetValue(name, out var result))
        {
            result = new Logger(name);
            loggersByName.Add(name, result);
        }
        return result;
    }

    public static void RegisterLog(ILog log)
    {
        logs.Add(log);
        if (log is ILogWithLifetime lifetimeLog)
        {
            lifetimeLog.Start();
        }
    }

    public static void CloseLog()
    {
        foreach (var log in logs)
        {
            if (log is ILogWithLifetime lifetimeLog)
            {
                lifetimeLog.End();
            }
        }
    }

    internal static void Log(Logger logger, LogLevel level, string message)
    {
        foreach (var log in logs)
        {
            log.Log(logger.Name, level, DateTime.Now, message);
        }
    }
}
