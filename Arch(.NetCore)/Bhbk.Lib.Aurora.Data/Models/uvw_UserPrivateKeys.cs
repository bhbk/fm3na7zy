using System;
using System.Collections.Generic;

namespace Bhbk.Lib.Aurora.Data.Models
{
    public partial class uvw_UserPrivateKeys
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid PublicKeyId { get; set; }
        public string KeyValueBase64 { get; set; }
        public string KeyValueAlgo { get; set; }
        public string KeyValuePass { get; set; }
        public bool Enabled { get; set; }
        public DateTime Created { get; set; }
        public bool Immutable { get; set; }
    }
}
