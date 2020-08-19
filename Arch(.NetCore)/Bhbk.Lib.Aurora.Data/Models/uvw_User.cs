using System;
using System.Collections.Generic;

namespace Bhbk.Lib.Aurora.Data.Models
{
    public partial class uvw_User
    {
        public Guid IdentityId { get; set; }
        public string IdentityAlias { get; set; }
        public bool RequirePassword { get; set; }
        public bool RequirePublicKey { get; set; }
        public string FileSystemType { get; set; }
        public bool FileSystemReadOnly { get; set; }
        public string DebugLevel { get; set; }
        public bool Enabled { get; set; }
        public bool Deletable { get; set; }
        public DateTime Created { get; set; }
        public DateTime? LastUpdated { get; set; }
    }
}
