using System;
using System.Collections.Generic;

#nullable disable

namespace Bhbk.Lib.Aurora.Data.ModelsMem
{
    public partial class UserFolderMem
    {
        public UserFolderMem()
        {
            Folders = new HashSet<UserFolderMem>();
            Files = new HashSet<UserFileMem>();
        }

        public Guid Id { get; set; }
        public Guid IdentityId { get; set; }
        public Guid? ParentId { get; set; }
        public string VirtualName { get; set; }
        public bool IsReadOnly { get; set; }
        public DateTimeOffset CreatedUtc { get; set; }
        public DateTimeOffset LastAccessedUtc { get; set; }
        public DateTimeOffset LastUpdatedUtc { get; set; }

        public virtual UserMem User { get; set; }
        public virtual UserFolderMem Parent { get; set; }
        public virtual ICollection<UserFolderMem> Folders { get; set; }
        public virtual ICollection<UserFileMem> Files { get; set; }
    }
}
