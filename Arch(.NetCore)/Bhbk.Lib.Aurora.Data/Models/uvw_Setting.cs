using System;
using System.Collections.Generic;

#nullable disable

namespace Bhbk.Lib.Aurora.Data.Models
{
    public partial class uvw_Setting
    {
        public Guid Id { get; set; }
        public Guid? IdentityId { get; set; }
        public string ConfigKey { get; set; }
        public string ConfigValue { get; set; }
        public bool IsDeletable { get; set; }
        public DateTimeOffset CreatedUtc { get; set; }
        public DateTimeOffset? LastUpdatedUtc { get; set; }
    }
}
