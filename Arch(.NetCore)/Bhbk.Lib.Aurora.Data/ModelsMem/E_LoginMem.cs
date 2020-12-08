using System;
using System.Collections.Generic;

#nullable disable

namespace Bhbk.Lib.Aurora.Data.ModelsMem
{
    public partial class E_LoginMem
    {
        public E_LoginMem()
        {
            Files = new HashSet<E_FileMem>();
            Folders = new HashSet<E_FolderMem>();
        }

        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public long QuotaInBytes { get; set; }
        public long QuotaUsedInBytes { get; set; }
        public virtual ICollection<E_FileMem> Files { get; set; }
        public virtual ICollection<E_FolderMem> Folders { get; set; }
    }
}
