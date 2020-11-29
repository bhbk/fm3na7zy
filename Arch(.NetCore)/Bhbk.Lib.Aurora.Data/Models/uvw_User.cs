using System;
using System.Collections.Generic;

#nullable disable

namespace Bhbk.Lib.Aurora.Data.Models
{
    public partial class uvw_User
    {
        public Guid IdentityId { get; set; }
        public string IdentityAlias { get; set; }
        public string FileSystemType { get; set; }
        public bool IsPasswordRequired { get; set; }
        public bool IsPublicKeyRequired { get; set; }
        public bool IsFileSystemReadOnly { get; set; }
        public string ChrootPath { get; set; }
        public long QuotaInBytes { get; set; }
        public long QuotaUsedInBytes { get; set; }
        public short ConcurrentSessions { get; set; }
        public string Debugger { get; set; }
        public bool IsEnabled { get; set; }
        public bool IsDeletable { get; set; }
        public DateTimeOffset CreatedUtc { get; set; }
        public DateTimeOffset? LastUpdatedUtc { get; set; }
    }
}
