using System;
using System.Collections.Generic;

#nullable disable

namespace Bhbk.Lib.Aurora.Data.ModelsMem
{
    public partial class FileSystemMem
    {
        public FileSystemMem()
        {
            Logins = new HashSet<FileSystemLoginMem>();
            Files = new HashSet<FileMem>();
            Folders = new HashSet<FolderMem>();
        }

        public Guid Id { get; set; }
        public int FileSystemTypeId { get; set; }

        public virtual FileSystemUsageMem Usage { get; set; }
        public virtual ICollection<FileSystemLoginMem> Logins { get; set; }
        public virtual ICollection<FileMem> Files { get; set; }
        public virtual ICollection<FolderMem> Folders { get; set; }
    }
}
