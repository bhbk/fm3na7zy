using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Bhbk.Lib.Aurora.Data_EF6.ModelsMem
{
    public partial class UserFileMem
    {
        [Key]
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
    
        public virtual UserLoginMem User { get; set; }
        public virtual UserFolderMem Parent { get; set; }
    }
}
