using System;
using System.Collections.Generic;

namespace Bhbk.Lib.Aurora.Data.EFCore.Models_DIRECT
{
    public partial class tbl_Users
    {
        public tbl_Users()
        {
            tbl_UserFiles = new HashSet<tbl_UserFiles>();
            tbl_UserFolders = new HashSet<tbl_UserFolders>();
            tbl_UserPrivateKeys = new HashSet<tbl_UserPrivateKeys>();
            tbl_UserPublicKeys = new HashSet<tbl_UserPublicKeys>();
        }

        public Guid Id { get; set; }
        public Guid? IdentityId { get; set; }
        public string UserName { get; set; }
        public string FileSystemType { get; set; }
        public bool FileSystemReadOnly { get; set; }
        public string DebugLevel { get; set; }
        public bool Enabled { get; set; }
        public DateTime Created { get; set; }
        public bool Immutable { get; set; }

        public virtual tbl_UserMounts tbl_UserMounts { get; set; }
        public virtual tbl_UserPasswords tbl_UserPasswords { get; set; }
        public virtual ICollection<tbl_UserFiles> tbl_UserFiles { get; set; }
        public virtual ICollection<tbl_UserFolders> tbl_UserFolders { get; set; }
        public virtual ICollection<tbl_UserPrivateKeys> tbl_UserPrivateKeys { get; set; }
        public virtual ICollection<tbl_UserPublicKeys> tbl_UserPublicKeys { get; set; }
    }
}
