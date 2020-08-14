using System;
using System.Collections.Generic;

namespace Bhbk.Lib.Aurora.Data.Models_DIRECT
{
    public partial class tbl_UserFolders
    {
        public tbl_UserFolders()
        {
            InverseParent = new HashSet<tbl_UserFolders>();
            tbl_UserFiles = new HashSet<tbl_UserFiles>();
        }

        public Guid Id { get; set; }
        public Guid IdentityId { get; set; }
        public Guid? ParentId { get; set; }
        public string VirtualName { get; set; }
        public bool ReadOnly { get; set; }
        public DateTime Created { get; set; }
        public DateTime? LastAccessed { get; set; }
        public DateTime? LastUpdated { get; set; }

        public virtual tbl_Users Identity { get; set; }
        public virtual tbl_UserFolders Parent { get; set; }
        public virtual ICollection<tbl_UserFolders> InverseParent { get; set; }
        public virtual ICollection<tbl_UserFiles> tbl_UserFiles { get; set; }
    }
}
