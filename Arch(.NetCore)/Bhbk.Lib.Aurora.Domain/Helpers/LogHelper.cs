using Bhbk.Lib.Aurora.Data_EF6.Models;
using Rebex;
using Serilog;
using System;

namespace Bhbk.Lib.Aurora.Domain.Helpers
{
    /*
     * https://forum.rebex.net/4157/logwriter-file-management
     */
    public class LogHelper : LogWriterBase
    {
        private readonly ILogger _logger;
        private readonly FileSystemLogin_EF _fileSystem;
        private const string MessageTemplate = "{type} {user} {id} {area} {message}";
        private const string MessageTemplateWithData = "{type} {user} {id} {area} {message} {data}";

        public LogHelper(ILogger logger, FileSystemLogin_EF fileSystem, LogLevel level)
        {
            Level = level;
            _fileSystem = fileSystem;
            _logger = logger;
        }

        private void Write(LogLevel level, Type objectType, FileSystemLogin_EF fileSystem, int objectId, string area, string message, ArraySegment<byte>? data)
        {
            string template = (data == null) ? MessageTemplate : MessageTemplateWithData;

            if (level <= LogLevel.Verbose)
                _logger.Verbose(template, objectType, _fileSystem.Login.UserName, objectId, area, message, data);

            else if (level <= LogLevel.Debug)
                _logger.Debug(template, objectType, _fileSystem.Login.UserName, objectId, area, message, data);

            else if (level <= LogLevel.Info)
                _logger.Information(template, objectType, _fileSystem.Login.UserName, objectId, area, message, data);

            else if (level <= LogLevel.Error)
                _logger.Error(template, objectType, _fileSystem.Login.UserName, objectId, area, message, data);
        }

        public override void Write(LogLevel level, Type objectType, int objectId, string area, string message)
        {
            Write(level, objectType, _fileSystem, objectId, area, message, null);
        }

        public override void Write(LogLevel level, Type objectType, int objectId, string area, string message, byte[] buffer, int offset, int length)
        {
            Write(level, objectType, _fileSystem, objectId, area, message, new ArraySegment<byte>(buffer, offset, length));
        }
    }
}
