using System;
using System.Collections.Generic;

#nullable disable

namespace Bhbk.Lib.Aurora.Data.ModelsMem
{
    public partial class FolderMem_EF
    {
        public FolderMem_EF()
        {
            Folders = new HashSet<FolderMem_EF>();
            Files = new HashSet<FileMem_EF>();
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

        public virtual LoginMem_EF User { get; set; }
        public virtual FolderMem_EF Parent { get; set; }
        public virtual ICollection<FolderMem_EF> Folders { get; set; }
        public virtual ICollection<FileMem_EF> Files { get; set; }
    }
}
