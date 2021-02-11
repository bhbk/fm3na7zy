using System;

namespace Bhbk.Lib.Aurora.Models
{
    public class PublicKey
    {
        public System.Guid Id { get; set; }
        public Nullable<System.Guid> UserId { get; set; }
        public Nullable<System.Guid> PrivateKeyId { get; set; }
        public string KeyValue { get; set; }
        public int KeyAlgorithmId { get; set; }
        public int KeyFormatId { get; set; }
        public string SigValue { get; set; }
        public int SigAlgorithmId { get; set; }
        public string Comment { get; set; }
        public System.DateTimeOffset CreatedUtc { get; set; }
        public bool IsEnabled { get; set; }
        public bool IsDeletable { get; set; }

        public virtual KeyAlgorithmType KeyAlgorithmType { get; set; }
        public virtual PublicKeyFormatType KeyFormatType { get; set; }
        public virtual PublicKeySignatureType KeySignatureType { get; set; }
        public virtual Login User { get; set; }
    }
}
