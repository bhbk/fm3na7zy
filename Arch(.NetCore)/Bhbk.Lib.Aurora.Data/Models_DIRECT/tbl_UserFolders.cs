using System;
using System.Collections.Generic;

namespace Bhbk.Lib.Aurora.Data.Models_DIRECT
{
    public partial class tbl_UserFolders
    {
        public tbl_UserFolders()
        {
            InverseVirtualParent = new HashSet<tbl_UserFolders>();
        }

        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid? VirtualParentId { get; set; }
        public string VirtualFolderName { get; set; }
        public DateTime Created { get; set; }
        public DateTime? LastAccessed { get; set; }
        public DateTime? LastUpdated { get; set; }

        public virtual tbl_Users User { get; set; }
        public virtual tbl_UserFolders VirtualParent { get; set; }
        public virtual ICollection<tbl_UserFolders> InverseVirtualParent { get; set; }
    }
}
