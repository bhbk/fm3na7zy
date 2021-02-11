using System;
using System.Collections.Generic;
using System.Text;

namespace Bhbk.Lib.Aurora.Models
{
    public class FileSystemLogin
    {
        public System.Guid UserId { get; set; }
        public System.Guid FileSystemId { get; set; }
        public Nullable<int> SmbAuthTypeId { get; set; }
        public Nullable<System.Guid> AmbassadorId { get; set; }
        public string ChrootPath { get; set; }
        public System.DateTimeOffset CreatedUtc { get; set; }
        public bool IsReadOnly { get; set; }

        public virtual Ambassador Ambassador { get; set; }
        public virtual FileSystem FileSystem { get; set; }
        public virtual SmbAuthType SmbAuthType { get; set; }
        public virtual Login User { get; set; }
    }
}
