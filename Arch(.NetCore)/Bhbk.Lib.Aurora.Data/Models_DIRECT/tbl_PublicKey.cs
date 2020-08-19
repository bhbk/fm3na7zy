using System;
using System.Collections.Generic;

namespace Bhbk.Lib.Aurora.Data.Models_DIRECT
{
    public partial class tbl_PublicKey
    {
        public Guid Id { get; set; }
        public Guid? IdentityId { get; set; }
        public Guid? PrivateKeyId { get; set; }
        public string KeyValue { get; set; }
        public string KeyAlgo { get; set; }
        public string KeyFormat { get; set; }
        public string SigValue { get; set; }
        public string SigAlgo { get; set; }
        public string Comment { get; set; }
        public bool Enabled { get; set; }
        public bool Deletable { get; set; }
        public DateTime Created { get; set; }
        public DateTime? LastUpdated { get; set; }

        public virtual tbl_User Identity { get; set; }
        public virtual tbl_PrivateKey PrivateKey { get; set; }
    }
}
