using System;
using System.Text;

namespace Jither.Logging
{
    public class Logger
    {
        public string Name { get; }

        internal Logger(string name)
        {
            this.Name = name;
        }

        public void Debug(string message)
        {
            if (LogProvider.Level <= LogLevel.Debug)
            {
                LogProvider.Log(this, LogLevel.Debug, message);
            }
        }

        public void Verbose(string message)
        {
            if (LogProvider.Level <= LogLevel.Verbose)
            {
                LogProvider.Log(this, LogLevel.Verbose, message);
            }
        }

        public void Info(string message)
        {
            if (LogProvider.Level <= LogLevel.Info)
            {
                LogProvider.Log(this, LogLevel.Info, message);
            }
        }

        public void Warning(string message)
        {
            if (LogProvider.Level <= LogLevel.Warning)
            {
                LogProvider.Log(this, LogLevel.Warning, message);
            }
        }

        public void Error(string message)
        {
            if (LogProvider.Level <= LogLevel.Error)
            {
                LogProvider.Log(this, LogLevel.Error, message);
            }
        }

        public void DebugWarning(string message)
        {
            if (LogProvider.Level <= LogLevel.Debug)
            {
                LogProvider.Log(this, LogLevel.Warning, message);
            }
        }
    }
}
