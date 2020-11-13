using System;
using System.Collections.Generic;

#nullable disable

namespace Bhbk.Lib.Aurora.Data.Models
{
    public partial class uvw_PrivateKey
    {
        public Guid Id { get; set; }
        public Guid? IdentityId { get; set; }
        public Guid PublicKeyId { get; set; }
        public string KeyValue { get; set; }
        public string KeyAlgo { get; set; }
        public string KeyPass { get; set; }
        public string KeyFormat { get; set; }
        public bool IsEnabled { get; set; }
        public bool IsDeletable { get; set; }
        public DateTimeOffset CreatedUtc { get; set; }
        public DateTimeOffset? LastUpdatedUtc { get; set; }
    }
}
