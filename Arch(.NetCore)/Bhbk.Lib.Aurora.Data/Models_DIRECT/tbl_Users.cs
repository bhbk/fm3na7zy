using System;
using System.Collections.Generic;

namespace Bhbk.Lib.Aurora.Data.Models_DIRECT
{
    public partial class tbl_Users
    {
        public tbl_Users()
        {
            tbl_UserPrivateKeys = new HashSet<tbl_UserPrivateKeys>();
            tbl_UserPublicKeys = new HashSet<tbl_UserPublicKeys>();
        }

        public Guid Id { get; set; }
        public Guid? IdentityId { get; set; }
        public string UserName { get; set; }
        public bool? Enabled { get; set; }
        public DateTime Created { get; set; }
        public bool Immutable { get; set; }

        public virtual tbl_UserPasswords tbl_UserPasswords { get; set; }
        public virtual ICollection<tbl_UserPrivateKeys> tbl_UserPrivateKeys { get; set; }
        public virtual ICollection<tbl_UserPublicKeys> tbl_UserPublicKeys { get; set; }
    }
}
