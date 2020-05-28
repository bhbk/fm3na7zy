using System;
using System.Collections.Generic;

namespace Bhbk.Lib.Aurora.Data.EFCore.Models_DIRECT
{
    public partial class tbl_UserMounts
    {
        public Guid UserId { get; set; }
        public Guid CredentialId { get; set; }
        public string AuthType { get; set; }
        public string ServerName { get; set; }
        public string ServerPath { get; set; }
        public bool Enabled { get; set; }
        public DateTime Created { get; set; }
        public bool Immutable { get; set; }

        public virtual tbl_SysCredentials Credential { get; set; }
        public virtual tbl_Users User { get; set; }
    }
}
