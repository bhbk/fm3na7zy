using System;
using System.Collections.Generic;
using System.Text;

namespace Bhbk.Lib.Aurora.Models
{
    public partial class LoginUsage
    {
        public System.Guid UserId { get; set; }
        public string UserName { get; set; }
        public short SessionMax { get; set; }
        public short SessionsInUse { get; set; }

        public virtual Login User { get; set; }
    }
}
