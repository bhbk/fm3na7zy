using System;

namespace Bhbk.Lib.Aurora.Models
{
    public class Alert
    {
        public System.Guid Id { get; set; }
        public System.Guid UserId { get; set; }
        public bool OnDelete { get; set; }
        public bool OnDownload { get; set; }
        public bool OnUpload { get; set; }
        public string ToDisplayName { get; set; }
        public string ToEmailAddress { get; set; }
        public string ToPhoneNumber { get; set; }
        public string Comment { get; set; }
        public System.DateTimeOffset CreatedUtc { get; set; }
        public bool IsEnabled { get; set; }

        public virtual Login User { get; set; }
    }
}
