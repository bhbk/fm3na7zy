using System;
using System.Collections.Generic;

#nullable disable

namespace Bhbk.Lib.Aurora.Data.Models
{
    public partial class uvw_LoginUsage
    {
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public long QuotaInBytes { get; set; }
        public long QuotaUsedInBytes { get; set; }
        public short SessionMax { get; set; }
        public short SessionsInUse { get; set; }
    }
}
