using System;
using System.Collections.Generic;

#nullable disable

namespace Bhbk.Lib.Aurora.Data.ModelsMem
{
    public partial class E_FileMem
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid FolderId { get; set; }
        public string VirtualName { get; set; }
        public byte[] Data { get; set; }
        public bool IsReadOnly { get; set; }
        public string HashSHA256 { get; set; }
        public DateTimeOffset CreatedUtc { get; set; }
        public DateTimeOffset LastAccessedUtc { get; set; }
        public DateTimeOffset LastUpdatedUtc { get; set; }

        public virtual E_FolderMem Folder { get; set; }
        public virtual E_LoginMem User { get; set; }
    }
}
