using System;
using System.Collections.Generic;

namespace Bhbk.Lib.Aurora.Models
{
    public class Ambassador
    {
        public System.Guid Id { get; set; }
        public string UserPrincipalName { get; set; }
        public string EncryptedPass { get; set; }
        public System.DateTimeOffset CreatedUtc { get; set; }
        public bool IsEnabled { get; set; }
        public bool IsDeletable { get; set; }

        public virtual ICollection<FileSystemLogin> FileSystems { get; set; }
    }
}
