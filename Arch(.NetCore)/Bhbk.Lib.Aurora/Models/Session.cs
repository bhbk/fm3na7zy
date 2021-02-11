using System;

namespace Bhbk.Lib.Aurora.Models
{
    public class Session
    {
        public System.Guid Id { get; set; }
        public Nullable<System.Guid> UserId { get; set; }
        public string UserName { get; set; }
        public string CallPath { get; set; }
        public string Details { get; set; }
        public string LocalEndPoint { get; set; }
        public string LocalSoftwareIdentifier { get; set; }
        public string RemoteEndPoint { get; set; }
        public string RemoteSoftwareIdentifier { get; set; }
        public System.DateTimeOffset CreatedUtc { get; set; }
        public bool IsActive { get; set; }

        public virtual Login User { get; set; }
    }
}
