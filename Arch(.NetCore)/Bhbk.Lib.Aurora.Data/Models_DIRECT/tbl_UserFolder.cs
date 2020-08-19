using System;
using System.Collections.Generic;

namespace Bhbk.Lib.Aurora.Data.Models_DIRECT
{
    public partial class tbl_UserFolder
    {
        public tbl_UserFolder()
        {
            InverseParent = new HashSet<tbl_UserFolder>();
            tbl_UserFile = new HashSet<tbl_UserFile>();
        }

        public Guid Id { get; set; }
        public Guid IdentityId { get; set; }
        public Guid? ParentId { get; set; }
        public string VirtualName { get; set; }
        public bool ReadOnly { get; set; }
        public DateTime Created { get; set; }
        public DateTime? LastAccessed { get; set; }
        public DateTime? LastUpdated { get; set; }

        public virtual tbl_User Identity { get; set; }
        public virtual tbl_UserFolder Parent { get; set; }
        public virtual ICollection<tbl_UserFolder> InverseParent { get; set; }
        public virtual ICollection<tbl_UserFile> tbl_UserFile { get; set; }
    }
}
