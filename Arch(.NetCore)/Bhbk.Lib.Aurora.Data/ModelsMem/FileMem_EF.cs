using System;
using System.Collections.Generic;

#nullable disable

namespace Bhbk.Lib.Aurora.Data.ModelsMem
{
    public partial class FileMem_EF
    {
        public Guid Id { get; set; }
        public Guid FileSystemId { get; set; }
        public Guid FolderId { get; set; }
        public string VirtualName { get; set; }
        public byte[] Data { get; set; }
        public bool IsReadOnly { get; set; }
        public string HashValue { get; set; }
        public Guid CreatorId { get; set; }
        public DateTimeOffset CreatedUtc { get; set; }
        public DateTimeOffset LastAccessedUtc { get; set; }
        public DateTimeOffset LastUpdatedUtc { get; set; }

        public virtual FolderMem_EF Folder { get; set; }
        public virtual LoginMem_EF User { get; set; }
    }
}
