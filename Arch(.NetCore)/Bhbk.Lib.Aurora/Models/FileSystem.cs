using System;
using System.Collections.Generic;
using System.Text;

namespace Bhbk.Lib.Aurora.Models
{
    public class FileSystem
    {
        public System.Guid Id { get; set; }
        public int FileSystemTypeId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string UncPath { get; set; }
        public System.DateTimeOffset CreatedUtc { get; set; }
        public bool IsEnabled { get; set; }
        public bool IsDeletable { get; set; }

        public virtual ICollection<FileData> Files { get; set; }
        public virtual FileSystemType FileSystemType { get; set; }
        public virtual ICollection<FolderData> Folders { get; set; }
        public virtual FileSystemUsage Usage { get; set; }
        public virtual ICollection<FileSystemLogin> Users { get; set; }
    }
}
