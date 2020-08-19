using System;
using System.Collections.Generic;

namespace Bhbk.Lib.Aurora.Data.Models
{
    public partial class uvw_UserFolder
    {
        public Guid Id { get; set; }
        public Guid IdentityId { get; set; }
        public Guid? ParentId { get; set; }
        public string VirtualName { get; set; }
        public bool ReadOnly { get; set; }
        public DateTime Created { get; set; }
        public DateTime? LastAccessed { get; set; }
        public DateTime? LastUpdated { get; set; }
    }
}
