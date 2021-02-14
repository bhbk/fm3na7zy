using System;
using System.Collections.Generic;

#nullable disable

namespace Bhbk.Lib.Aurora.Data.ModelsMem
{
    public partial class LoginMem
    {
        public LoginMem()
        {
            FilesCreated = new HashSet<FileMem>();
            FoldersCreated = new HashSet<FolderMem>();
        }

        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public virtual ICollection<FileMem> FilesCreated { get; set; }
        public virtual ICollection<FolderMem> FoldersCreated { get; set; }
    }
}
