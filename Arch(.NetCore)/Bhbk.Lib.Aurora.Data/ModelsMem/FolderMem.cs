using System;
using System.Collections.Generic;

#nullable disable

namespace Bhbk.Lib.Aurora.Data.ModelsMem
{
    public partial class FolderMem
    {
        public FolderMem()
        {
            Folders = new HashSet<FolderMem>();
            Files = new HashSet<FileMem>();
        }

        public Guid Id { get; set; }
        public Guid FileSystemId { get; set; }
        public Guid? ParentId { get; set; }
        public string VirtualName { get; set; }
        public bool IsReadOnly { get; set; }
        public Guid CreatorId { get; set; }
        public DateTimeOffset CreatedUtc { get; set; }
        public DateTimeOffset LastAccessedUtc { get; set; }
        public DateTimeOffset LastUpdatedUtc { get; set; }

        public virtual LoginMem Creator { get; set; }
        public virtual FileSystemMem FileSystem { get; set; }
        public virtual FolderMem Parent { get; set; }
        public virtual ICollection<FolderMem> Folders { get; set; }
        public virtual ICollection<FileMem> Files { get; set; }
    }
}
