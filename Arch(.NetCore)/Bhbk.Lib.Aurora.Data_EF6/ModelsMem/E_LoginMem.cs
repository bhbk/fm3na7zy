using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Bhbk.Lib.Aurora.Data_EF6.ModelsMem
{
    public partial class E_LoginMem
    {
        public E_LoginMem()
        {
            this.Files = new HashSet<E_FileMem>();
            this.Folders = new HashSet<E_FolderMem>();
        }
    
        [Key]
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public long QuotaInBytes { get; set; }
        public long QuotaUsedInBytes { get; set; }

        public virtual ICollection<E_FileMem> Files { get; set; }
        public virtual ICollection<E_FolderMem> Folders { get; set; }
    }
}
