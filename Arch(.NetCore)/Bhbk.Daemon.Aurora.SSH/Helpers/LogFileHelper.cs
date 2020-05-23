using Rebex;
using Serilog;
using System;

namespace Bhbk.Daemon.Aurora.SSH.Helpers
{
    /*
     * the serilog framework handles log rotation, etc. the rebex framework does not. override the latter
     * and call the former so we do not have to muck with log rotation, etc.
     */
    public class LogFileHelper : ILogWriter
    {
        public LogLevel Level { get; set; }

        public LogFileHelper(LogLevel level)
        {
            Level = level;
        }

        public void Write(LogLevel level, Type objectType, int objectId, string area, string message)
        {
            switch (Level)
            {
                case LogLevel.Info:
                    Log.Information($"{objectType} {objectId} {area} {message}");
                    break;
                case LogLevel.Error:
                    Log.Error($"{objectType} {objectId} {area} {message}");
                    break;
                case LogLevel.Debug:
                    Log.Debug($"{objectType} {objectId} {area} {message}");
                    break;
                case LogLevel.Verbose:
                    Log.Verbose($"{objectType} {objectId} {area} {message}");
                    break;
                case LogLevel.Off:
                    break;
            }
        }

        public void Write(LogLevel level, Type objectType, int objectId, string area, string message, byte[] buffer, int offset, int length)
        {
            switch (Level)
            {
                case LogLevel.Info:
                    Log.Information($"{objectType} {objectId} {area} {message} {buffer} {offset} {length}");
                    break;
                case LogLevel.Error:
                    Log.Error($"{objectType} {objectId} {area} {message} {buffer} {offset} {length}");
                    break;
                case LogLevel.Debug:
                    Log.Debug($"{objectType} {objectId} {area} {message} {buffer} {offset} {length}");
                    break;
                case LogLevel.Verbose:
                    Log.Verbose($"{objectType} {objectId} {area} {message} {buffer} {offset} {length}");
                    break;
                case LogLevel.Off:
                    break;
            }
        }
    }
}
