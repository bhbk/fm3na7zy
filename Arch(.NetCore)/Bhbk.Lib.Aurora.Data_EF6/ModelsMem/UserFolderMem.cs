using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Bhbk.Lib.Aurora.Data_EF6.ModelsMem
{
    public partial class UserFolderMem
    {
        public UserFolderMem()
        {
            this.Files = new HashSet<UserFileMem>();
            this.Folders = new HashSet<UserFolderMem>();
        }
    
        [Key]
        public Guid Id { get; set; }
        public Guid IdentityId { get; set; }
        public Nullable<Guid> ParentId { get; set; }
        public string VirtualName { get; set; }
        public bool IsReadOnly { get; set; }
        public DateTimeOffset CreatedUtc { get; set; }
        public DateTimeOffset LastAccessedUtc { get; set; }
        public DateTimeOffset LastUpdatedUtc { get; set; }
    
        public virtual UserMem User { get; set; }
        public virtual ICollection<UserFileMem> Files { get; set; }
        public virtual ICollection<UserFolderMem> Folders { get; set; }
        public virtual UserFolderMem Parent { get; set; }
    }
}
