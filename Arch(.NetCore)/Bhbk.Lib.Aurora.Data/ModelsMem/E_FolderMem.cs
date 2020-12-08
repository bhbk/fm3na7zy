using System;
using System.Collections.Generic;

#nullable disable

namespace Bhbk.Lib.Aurora.Data.ModelsMem
{
    public partial class E_FolderMem
    {
        public E_FolderMem()
        {
            Folders = new HashSet<E_FolderMem>();
            Files = new HashSet<E_FileMem>();
        }

        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid? ParentId { get; set; }
        public string VirtualName { get; set; }
        public bool IsReadOnly { get; set; }
        public DateTimeOffset CreatedUtc { get; set; }
        public DateTimeOffset LastAccessedUtc { get; set; }
        public DateTimeOffset LastUpdatedUtc { get; set; }

        public virtual E_LoginMem User { get; set; }
        public virtual E_FolderMem Parent { get; set; }
        public virtual ICollection<E_FolderMem> Folders { get; set; }
        public virtual ICollection<E_FileMem> Files { get; set; }
    }
}
