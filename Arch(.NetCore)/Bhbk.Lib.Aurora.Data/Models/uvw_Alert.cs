using System;
using System.Collections.Generic;

#nullable disable

namespace Bhbk.Lib.Aurora.Data.Models
{
    public partial class uvw_Alert
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public bool OnDelete { get; set; }
        public bool OnDownload { get; set; }
        public bool OnUpload { get; set; }
        public string ToDisplayName { get; set; }
        public string ToEmailAddress { get; set; }
        public string ToPhoneNumber { get; set; }
        public bool IsEnabled { get; set; }
        public DateTimeOffset CreatedUtc { get; set; }
        public DateTimeOffset? LastUpdatedUtc { get; set; }
    }
}
