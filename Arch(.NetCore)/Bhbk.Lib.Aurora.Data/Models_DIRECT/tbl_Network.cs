using System;
using System.Collections.Generic;

#nullable disable

namespace Bhbk.Lib.Aurora.Data.Models_DIRECT
{
    public partial class tbl_Network
    {
        public Guid Id { get; set; }
        public Guid IdentityId { get; set; }
        public string Address { get; set; }
        public string Action { get; set; }
        public bool IsEnabled { get; set; }
        public DateTimeOffset CreatedUtc { get; set; }
        public DateTimeOffset? LastUpdatedUtc { get; set; }

        public virtual tbl_User Identity { get; set; }
    }
}
