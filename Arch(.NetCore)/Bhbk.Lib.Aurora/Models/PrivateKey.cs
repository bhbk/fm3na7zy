using System;

namespace Bhbk.Lib.Aurora.Models
{
    public class PrivateKey
    {
        public System.Guid Id { get; set; }
        public Nullable<System.Guid> UserId { get; set; }
        public System.Guid PublicKeyId { get; set; }
        public string KeyValue { get; set; }
        public int KeyAlgorithmId { get; set; }
        public int KeyFormatId { get; set; }
        public string EncryptedPass { get; set; }
        public System.DateTimeOffset CreatedUtc { get; set; }
        public bool IsEnabled { get; set; }
        public bool IsDeletable { get; set; }

        public virtual KeyAlgorithmType KeyAlgorithmType { get; set; }
        public virtual PrivateKeyFormatType KeyFormatType { get; set; }
        public virtual Login User { get; set; }
    }
}
