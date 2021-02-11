using System;
using System.Collections.Generic;
using System.Text;

namespace Bhbk.Lib.Aurora.Models
{
    public partial class FileSystemUsage
    {
        public System.Guid FileSystemId { get; set; }
        public string FileSystemName { get; set; }
        public long QuotaInBytes { get; set; }
        public long QuotaUsedInBytes { get; set; }

        public virtual FileSystem FileSystem { get; set; }
    }
}
