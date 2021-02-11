using System;
using System.Collections.Generic;

namespace Bhbk.Lib.Aurora.Models
{
    public class Login
    {
        public System.Guid UserId { get; set; }
        public string UserName { get; set; }
        public int AuthTypeId { get; set; }
        public bool IsPasswordRequired { get; set; }
        public bool IsPublicKeyRequired { get; set; }
        public string Comment { get; set; }
        public string EncryptedPass { get; set; }
        public int DebugTypeId { get; set; }
        public System.DateTimeOffset CreatedUtc { get; set; }
        public bool IsEnabled { get; set; }
        public bool IsDeletable { get; set; }

        public virtual ICollection<Alert> Alerts { get; set; }
        public virtual LoginAuthType AuthType { get; set; }
        public virtual LoginDebugType DebugType { get; set; }
        public virtual ICollection<Network> Networks { get; set; }
        public virtual ICollection<FileSystemLogin> FileSystems { get; set; }
        public virtual ICollection<PrivateKey> PrivateKeys { get; set; }
        public virtual ICollection<PublicKey> PublicKeys { get; set; }
        public virtual ICollection<Session> Sessions { get; set; }
        public virtual ICollection<Setting> Settings { get; set; }
        public virtual LoginUsage Usage { get; set; }
    }
}
