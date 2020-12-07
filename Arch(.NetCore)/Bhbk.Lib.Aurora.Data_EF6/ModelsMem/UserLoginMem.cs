using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Bhbk.Lib.Aurora.Data_EF6.ModelsMem
{
    public partial class UserLoginMem
    {
        public UserLoginMem()
        {
            this.Files = new HashSet<UserFileMem>();
            this.Folders = new HashSet<UserFolderMem>();
        }
    
        [Key]
        public Guid IdentityId { get; set; }
        public string IdentityAlias { get; set; }
        public long QuotaInBytes { get; set; }
        public long QuotaUsedInBytes { get; set; }

        public virtual ICollection<UserFileMem> Files { get; set; }
        public virtual ICollection<UserFolderMem> Folders { get; set; }
    }
}
