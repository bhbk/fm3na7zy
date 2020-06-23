using System;
using System.Collections.Generic;

namespace Bhbk.Lib.Aurora.Data.Models
{
    public partial class uvw_UserMounts
    {
        public Guid UserId { get; set; }
        public Guid CredentialId { get; set; }
        public string AuthType { get; set; }
        public string ServerName { get; set; }
        public string ServerPath { get; set; }
        public bool Enabled { get; set; }
        public DateTime Created { get; set; }
        public bool Immutable { get; set; }
    }
}
