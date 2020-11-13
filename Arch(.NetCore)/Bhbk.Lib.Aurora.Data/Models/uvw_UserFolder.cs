using System;
using System.Collections.Generic;

#nullable disable

namespace Bhbk.Lib.Aurora.Data.Models
{
    public partial class uvw_UserFolder
    {
        public Guid Id { get; set; }
        public Guid IdentityId { get; set; }
        public Guid? ParentId { get; set; }
        public string VirtualName { get; set; }
        public bool IsReadOnly { get; set; }
        public DateTimeOffset CreatedUtc { get; set; }
        public DateTimeOffset? LastAccessedUtc { get; set; }
        public DateTimeOffset? LastUpdatedUtc { get; set; }
    }
}
