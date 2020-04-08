using System;
using System.Collections.Generic;

namespace Bhbk.Lib.Aurora.Data.EFCore.Models_DIRECT
{
    public partial class tbl_UserPrivateKeys
    {
        public tbl_UserPrivateKeys()
        {
            tbl_UserPublicKeys = new HashSet<tbl_UserPublicKeys>();
        }

        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid PublicKeyId { get; set; }
        public string KeyValueBase64 { get; set; }
        public string KeyValueAlgo { get; set; }
        public string KeyValuePass { get; set; }
        public bool Enabled { get; set; }
        public DateTime Created { get; set; }
        public bool Immutable { get; set; }

        public virtual tbl_Users User { get; set; }
        public virtual ICollection<tbl_UserPublicKeys> tbl_UserPublicKeys { get; set; }
    }
}
