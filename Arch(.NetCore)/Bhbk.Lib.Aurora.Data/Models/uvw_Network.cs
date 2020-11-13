using System;
using System.Collections.Generic;

#nullable disable

namespace Bhbk.Lib.Aurora.Data.Models
{
    public partial class uvw_Network
    {
        public Guid Id { get; set; }
        public Guid IdentityId { get; set; }
        public string Address { get; set; }
        public string Action { get; set; }
        public bool IsEnabled { get; set; }
        public DateTimeOffset CreatedUtc { get; set; }
        public DateTimeOffset? LastUpdatedUtc { get; set; }
    }
}
