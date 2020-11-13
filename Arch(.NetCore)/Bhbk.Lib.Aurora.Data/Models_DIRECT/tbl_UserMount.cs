using System;
using System.Collections.Generic;

#nullable disable

namespace Bhbk.Lib.Aurora.Data.Models_DIRECT
{
    public partial class tbl_UserMount
    {
        public Guid IdentityId { get; set; }
        public Guid? CredentialId { get; set; }
        public string AuthType { get; set; }
        public string ServerAddress { get; set; }
        public string ServerShare { get; set; }
        public bool IsEnabled { get; set; }
        public bool IsDeletable { get; set; }
        public DateTimeOffset CreatedUtc { get; set; }
        public DateTimeOffset? LastUpdatedUtc { get; set; }

        public virtual tbl_Credential Credential { get; set; }
        public virtual tbl_User Identity { get; set; }
    }
}
