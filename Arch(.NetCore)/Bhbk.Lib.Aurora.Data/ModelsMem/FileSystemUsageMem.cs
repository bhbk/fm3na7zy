using System;
using System.Collections.Generic;

#nullable disable

namespace Bhbk.Lib.Aurora.Data.ModelsMem
{
    public partial class FileSystemUsageMem
    {
        public Guid FileSystemId { get; set; }
        public long QuotaInBytes { get; set; }
        public long QuotaUsedInBytes { get; set; }

        public virtual FileSystemMem FileSystem { get; set; }
    }
}
