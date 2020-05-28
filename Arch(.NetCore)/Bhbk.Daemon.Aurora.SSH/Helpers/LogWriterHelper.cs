﻿using Bhbk.Lib.Aurora.Data.EFCore.Models_DIRECT;
using Rebex;
using Serilog;
using System;

namespace Bhbk.Daemon.Aurora.SSH.Helpers
{
    /*
     * https://forum.rebex.net/4157/logwriter-file-management
     */
    public class LogWriterHelper : LogWriterBase
    {
        private readonly ILogger _logger;
        private readonly tbl_Users _user;
        private const string MessageTemplate = "{type} {user} {id} {area} {message}";
        private const string MessageTemplateWithData = "{type} {user} {id} {area} {message} {data}";

        public LogWriterHelper(ILogger logger, tbl_Users user, LogLevel level)
        {
            Level = level;
            _user = user;
            _logger = logger;
        }

        private void Write(LogLevel level, Type objectType, tbl_Users user, int objectId, string area, string message, ArraySegment<byte>? data)
        {
            string template = (data == null) ? MessageTemplate : MessageTemplateWithData;

            if (level <= LogLevel.Verbose)
                _logger.Verbose(template, objectType, _user.UserName, objectId, area, message, data);

            else if (level <= LogLevel.Debug)
                _logger.Debug(template, objectType, _user.UserName, objectId, area, message, data);

            else if (level <= LogLevel.Info)
                _logger.Information(template, objectType, _user.UserName, objectId, area, message, data);

            else if (level <= LogLevel.Error)
                _logger.Error(template, objectType, _user.UserName, objectId, area, message, data);
        }

        public override void Write(LogLevel level, Type objectType, int objectId, string area, string message)
        {
            Write(level, objectType, _user, objectId, area, message, null);
        }

        public override void Write(LogLevel level, Type objectType, int objectId, string area, string message, byte[] buffer, int offset, int length)
        {
            Write(level, objectType, _user, objectId, area, message, new ArraySegment<byte>(buffer, offset, length));
        }
    }
}
