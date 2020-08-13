using System;
using System.Collections.Generic;

namespace Bhbk.Lib.Aurora.Data.Models
{
    public partial class uvw_PrivateKeys
    {
        public Guid Id { get; set; }
        public Guid? UserId { get; set; }
        public Guid? PublicKeyId { get; set; }
        public string KeyValue { get; set; }
        public string KeyAlgo { get; set; }
        public string KeyPass { get; set; }
        public string KeyFormat { get; set; }
        public bool Enabled { get; set; }
        public DateTime Created { get; set; }
        public DateTime LastUpdated { get; set; }
        public bool Immutable { get; set; }
    }
}
