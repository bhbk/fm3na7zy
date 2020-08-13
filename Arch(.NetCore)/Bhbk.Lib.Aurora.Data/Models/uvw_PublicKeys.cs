using System;
using System.Collections.Generic;

namespace Bhbk.Lib.Aurora.Data.Models
{
    public partial class uvw_PublicKeys
    {
        public Guid Id { get; set; }
        public Guid? UserId { get; set; }
        public Guid? PrivateKeyId { get; set; }
        public string KeyValue { get; set; }
        public string KeyAlgo { get; set; }
        public string KeyFormat { get; set; }
        public string SigValue { get; set; }
        public string SigAlgo { get; set; }
        public string Hostname { get; set; }
        public bool Enabled { get; set; }
        public DateTime Created { get; set; }
        public DateTime LastUpdated { get; set; }
        public bool Immutable { get; set; }
    }
}
