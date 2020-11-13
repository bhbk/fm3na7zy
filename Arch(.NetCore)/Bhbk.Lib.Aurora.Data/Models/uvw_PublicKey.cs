using System;
using System.Collections.Generic;

#nullable disable

namespace Bhbk.Lib.Aurora.Data.Models
{
    public partial class uvw_PublicKey
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
        public bool IsEnabled { get; set; }
        public bool IsDeletable { get; set; }
        public DateTimeOffset CreatedUtc { get; set; }
        public DateTimeOffset? LastUpdatedUtc { get; set; }
    }
}
