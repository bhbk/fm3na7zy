using System;
using System.Collections.Generic;

namespace Bhbk.Lib.Aurora.Data.Models_DIRECT
{
    public partial class tbl_Users
    {
        public tbl_Users()
        {
            tbl_Networks = new HashSet<tbl_Networks>();
            tbl_PrivateKeys = new HashSet<tbl_PrivateKeys>();
            tbl_PublicKeys = new HashSet<tbl_PublicKeys>();
            tbl_Settings = new HashSet<tbl_Settings>();
            tbl_UserFiles = new HashSet<tbl_UserFiles>();
            tbl_UserFolders = new HashSet<tbl_UserFolders>();
        }

        public Guid IdentityId { get; set; }
        public string IdentityAlias { get; set; }
        public bool RequirePassword { get; set; }
        public bool RequirePublicKey { get; set; }
        public string FileSystemType { get; set; }
        public bool FileSystemReadOnly { get; set; }
        public string DebugLevel { get; set; }
        public bool Enabled { get; set; }
        public DateTime Created { get; set; }
        public bool Immutable { get; set; }

        public virtual tbl_UserMounts tbl_UserMounts { get; set; }
        public virtual ICollection<tbl_Networks> tbl_Networks { get; set; }
        public virtual ICollection<tbl_PrivateKeys> tbl_PrivateKeys { get; set; }
        public virtual ICollection<tbl_PublicKeys> tbl_PublicKeys { get; set; }
        public virtual ICollection<tbl_Settings> tbl_Settings { get; set; }
        public virtual ICollection<tbl_UserFiles> tbl_UserFiles { get; set; }
        public virtual ICollection<tbl_UserFolders> tbl_UserFolders { get; set; }
    }
}
