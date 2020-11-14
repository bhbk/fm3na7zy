using System;
using System.Collections.Generic;

#nullable disable

namespace Bhbk.Lib.Aurora.Data.Models_DIRECT
{
    public partial class tbl_UserAlert
    {
        public Guid Id { get; set; }
        public Guid IdentityId { get; set; }
        public bool OnDelete { get; set; }
        public bool OnDownload { get; set; }
        public bool OnUpload { get; set; }
        public string ToFirstName { get; set; }
        public string ToLastName { get; set; }
        public string ToEmailAddress { get; set; }
        public string ToPhoneNumber { get; set; }
        public DateTimeOffset CreatedUtc { get; set; }
        public DateTimeOffset? LastUpdatedUtc { get; set; }

        public virtual tbl_User Identity { get; set; }
    }
}
