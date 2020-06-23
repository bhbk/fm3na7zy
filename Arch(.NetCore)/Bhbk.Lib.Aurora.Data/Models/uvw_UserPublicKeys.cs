using System;
using System.Collections.Generic;

namespace Bhbk.Lib.Aurora.Data.Models
{
    public partial class uvw_UserPublicKeys
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid? PrivateKeyId { get; set; }
        public string KeyValueBase64 { get; set; }
        public string KeyValueAlgo { get; set; }
        public string KeySig { get; set; }
        public string KeySigAlgo { get; set; }
        public string Hostname { get; set; }
        public bool Enabled { get; set; }
        public DateTime Created { get; set; }
        public bool Immutable { get; set; }
    }
}
