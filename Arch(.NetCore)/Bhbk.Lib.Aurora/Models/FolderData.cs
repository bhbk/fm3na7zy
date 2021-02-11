using System;
using System.Collections.Generic;

namespace Bhbk.Lib.Aurora.Models
{
    public class FolderData
    {
        public System.Guid Id { get; set; }
        public System.Guid FileSystemId { get; set; }
        public Nullable<System.Guid> ParentId { get; set; }
        public string VirtualName { get; set; }
        public bool IsReadOnly { get; set; }
        public System.Guid CreatedByUserId { get; set; }
        public string CreatedByUserName { get; set; }
        public System.DateTimeOffset CreatedUtc { get; set; }
        public System.DateTimeOffset LastAccessedUtc { get; set; }
        public System.DateTimeOffset LastUpdatedUtc { get; set; }

        public virtual Login Creator { get; set; }
        public virtual FileSystem FileSystem { get; set; }
        public virtual ICollection<FileData> Files { get; set; }
        public virtual ICollection<FolderData> Folders { get; set; }
        public virtual FolderData Parent { get; set; }
    }
}
