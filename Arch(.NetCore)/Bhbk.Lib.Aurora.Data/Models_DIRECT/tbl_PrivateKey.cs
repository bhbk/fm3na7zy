using System;
using System.Collections.Generic;

namespace Bhbk.Lib.Aurora.Data.Models_DIRECT
{
    public partial class tbl_PrivateKey
    {
        public tbl_PrivateKey()
        {
            tbl_PublicKey = new HashSet<tbl_PublicKey>();
        }

        public Guid Id { get; set; }
        public Guid? IdentityId { get; set; }
        public Guid PublicKeyId { get; set; }
        public string KeyValue { get; set; }
        public string KeyAlgo { get; set; }
        public string KeyPass { get; set; }
        public string KeyFormat { get; set; }
        public bool Enabled { get; set; }
        public bool Deletable { get; set; }
        public DateTime Created { get; set; }
        public DateTime? LastUpdated { get; set; }

        public virtual tbl_User Identity { get; set; }
        public virtual ICollection<tbl_PublicKey> tbl_PublicKey { get; set; }
    }
}
