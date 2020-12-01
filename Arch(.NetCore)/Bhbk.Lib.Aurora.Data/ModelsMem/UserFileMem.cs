using System;
using System.Collections.Generic;

#nullable disable

namespace Bhbk.Lib.Aurora.Data.ModelsMem
{
    public partial class UserFileMem
    {
        public Guid Id { get; set; }
        public Guid IdentityId { get; set; }
        public Guid FolderId { get; set; }
        public string VirtualName { get; set; }
        public byte[] Data { get; set; }
        public bool IsReadOnly { get; set; }
        public string HashSHA256 { get; set; }
        public DateTimeOffset CreatedUtc { get; set; }
        public DateTimeOffset LastAccessedUtc { get; set; }
        public DateTimeOffset LastUpdatedUtc { get; set; }

        public virtual UserFolderMem Folder { get; set; }
        public virtual UserMem User { get; set; }
    }
}
