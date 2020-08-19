using System;
using System.Collections.Generic;

namespace Bhbk.Lib.Aurora.Data.Models
{
    public partial class uvw_Network
    {
        public Guid Id { get; set; }
        public Guid IdentityId { get; set; }
        public string Address { get; set; }
        public string Action { get; set; }
        public bool Enabled { get; set; }
        public DateTime Created { get; set; }
        public DateTime? LastUpdated { get; set; }
    }
}
