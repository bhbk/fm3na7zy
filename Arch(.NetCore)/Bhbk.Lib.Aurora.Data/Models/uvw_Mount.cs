using System;
using System.Collections.Generic;

#nullable disable

namespace Bhbk.Lib.Aurora.Data.Models
{
    public partial class uvw_Mount
    {
        public Guid UserId { get; set; }
        public Guid? AmbassadorId { get; set; }
        public string AuthType { get; set; }
        public string ServerAddress { get; set; }
        public string ServerShare { get; set; }
        public bool IsEnabled { get; set; }
        public bool IsDeletable { get; set; }
        public DateTimeOffset CreatedUtc { get; set; }
        public DateTimeOffset? LastUpdatedUtc { get; set; }
    }
}
