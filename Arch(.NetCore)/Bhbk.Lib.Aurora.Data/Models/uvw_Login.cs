using System;
using System.Collections.Generic;

#nullable disable

namespace Bhbk.Lib.Aurora.Data.Models
{
    public partial class uvw_Login
    {
        public Guid UserId { get; set; }
        public string UserLoginType { get; set; }
        public string UserName { get; set; }
        public string FileSystemType { get; set; }
        public string FileSystemChrootPath { get; set; }
        public bool IsPasswordRequired { get; set; }
        public bool IsPublicKeyRequired { get; set; }
        public bool IsFileSystemReadOnly { get; set; }
        public string Debugger { get; set; }
        public string EncryptedPass { get; set; }
        public bool IsEnabled { get; set; }
        public bool IsDeletable { get; set; }
        public DateTimeOffset CreatedUtc { get; set; }
        public DateTimeOffset? LastUpdatedUtc { get; set; }
    }
}
