using System;
using System.Collections.Generic;

namespace Bhbk.Lib.Aurora.Data.Models_DIRECT
{
    public partial class tbl_Networks
    {
        public Guid Id { get; set; }
        public Guid IdentityId { get; set; }
        public string Address { get; set; }
        public string Action { get; set; }
        public bool Enabled { get; set; }
        public DateTime Created { get; set; }
        public DateTime? LastUpdated { get; set; }

        public virtual tbl_Users Identity { get; set; }
    }
}
