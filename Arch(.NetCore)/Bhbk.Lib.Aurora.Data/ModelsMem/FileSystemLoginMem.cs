using System;
using System.Collections.Generic;

#nullable disable

namespace Bhbk.Lib.Aurora.Data.ModelsMem
{
    public partial class FileSystemLoginMem
    {
        public Guid FileSystemId { get; set; }
        public Guid UserId { get; set; }

        public virtual FileSystemMem FileSystem { get; set; }
        public virtual LoginMem User { get; set; }
    }
}
