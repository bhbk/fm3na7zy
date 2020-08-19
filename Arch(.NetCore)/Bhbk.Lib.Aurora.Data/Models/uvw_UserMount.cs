using System;
using System.Collections.Generic;

namespace Bhbk.Lib.Aurora.Data.Models
{
    public partial class uvw_UserMount
    {
        public Guid IdentityId { get; set; }
        public Guid? CredentialId { get; set; }
        public string AuthType { get; set; }
        public string ServerAddress { get; set; }
        public string ServerShare { get; set; }
        public bool Enabled { get; set; }
        public bool Deletable { get; set; }
        public DateTime Created { get; set; }
        public DateTime? LastUpdated { get; set; }
    }
}
