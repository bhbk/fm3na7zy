using System;
using System.Collections.Generic;

#nullable disable

namespace Bhbk.Lib.Aurora.Data.ModelsMem
{
    public partial class LoginMem_EF
    {
        public LoginMem_EF()
        {
            Files = new HashSet<FileMem_EF>();
            Folders = new HashSet<FolderMem_EF>();
        }

        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public long QuotaInBytes { get; set; }
        public long QuotaUsedInBytes { get; set; }
        public virtual ICollection<FileMem_EF> Files { get; set; }
        public virtual ICollection<FolderMem_EF> Folders { get; set; }
    }
}
