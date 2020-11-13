using System;
using System.Collections.Generic;

#nullable disable

namespace Bhbk.Lib.Aurora.Data.Models_DIRECT
{
    public partial class tbl_User
    {
        public tbl_User()
        {
            tbl_Activities = new HashSet<tbl_Activity>();
            tbl_Networks = new HashSet<tbl_Network>();
            tbl_PrivateKeys = new HashSet<tbl_PrivateKey>();
            tbl_PublicKeys = new HashSet<tbl_PublicKey>();
            tbl_Settings = new HashSet<tbl_Setting>();
            tbl_UserFiles = new HashSet<tbl_UserFile>();
            tbl_UserFolders = new HashSet<tbl_UserFolder>();
        }

        public Guid IdentityId { get; set; }
        public string IdentityAlias { get; set; }
        public bool RequirePassword { get; set; }
        public bool RequirePublicKey { get; set; }
        public string FileSystemType { get; set; }
        public bool FileSystemReadOnly { get; set; }
        public string DebugLevel { get; set; }
        public bool IsEnabled { get; set; }
        public bool IsDeletable { get; set; }
        public DateTimeOffset CreatedUtc { get; set; }
        public DateTimeOffset? LastUpdatedUtc { get; set; }

        public virtual tbl_UserMount tbl_UserMount { get; set; }
        public virtual ICollection<tbl_Activity> tbl_Activities { get; set; }
        public virtual ICollection<tbl_Network> tbl_Networks { get; set; }
        public virtual ICollection<tbl_PrivateKey> tbl_PrivateKeys { get; set; }
        public virtual ICollection<tbl_PublicKey> tbl_PublicKeys { get; set; }
        public virtual ICollection<tbl_Setting> tbl_Settings { get; set; }
        public virtual ICollection<tbl_UserFile> tbl_UserFiles { get; set; }
        public virtual ICollection<tbl_UserFolder> tbl_UserFolders { get; set; }
    }
}
