using System;
using System.Collections.Generic;

#nullable disable

namespace Bhbk.Lib.Aurora.Data.Models
{
    public partial class uvw_Activity
    {
        public Guid Id { get; set; }
        public Guid? ActorId { get; set; }
        public Guid? IdentityId { get; set; }
        public string ActivityType { get; set; }
        public string TableName { get; set; }
        public string KeyValues { get; set; }
        public string OriginalValues { get; set; }
        public bool IsDeletable { get; set; }
        public string CurrentValues { get; set; }
        public DateTimeOffset CreatedUtc { get; set; }
    }
}
