using System;
using System.Collections.Generic;

namespace Bhbk.Lib.Aurora.Data.Models_DIRECT
{
    public partial class tbl_User
    {
        public tbl_User()
        {
            tbl_Network = new HashSet<tbl_Network>();
            tbl_PrivateKey = new HashSet<tbl_PrivateKey>();
            tbl_PublicKey = new HashSet<tbl_PublicKey>();
            tbl_Setting = new HashSet<tbl_Setting>();
            tbl_UserFile = new HashSet<tbl_UserFile>();
            tbl_UserFolder = new HashSet<tbl_UserFolder>();
        }

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

        public virtual tbl_UserMount tbl_UserMount { get; set; }
        public virtual ICollection<tbl_Network> tbl_Network { get; set; }
        public virtual ICollection<tbl_PrivateKey> tbl_PrivateKey { get; set; }
        public virtual ICollection<tbl_PublicKey> tbl_PublicKey { get; set; }
        public virtual ICollection<tbl_Setting> tbl_Setting { get; set; }
        public virtual ICollection<tbl_UserFile> tbl_UserFile { get; set; }
        public virtual ICollection<tbl_UserFolder> tbl_UserFolder { get; set; }
    }
}
