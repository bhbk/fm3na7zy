using System;
using System.Collections.Generic;

#nullable disable

namespace Bhbk.Lib.Aurora.Data.Models
{
    public partial class uvw_Session
    {
        public Guid Id { get; set; }
        public Guid? IdentityId { get; set; }
        public string IdentityAlias { get; set; }
        public string CallPath { get; set; }
        public string Details { get; set; }
        public string LocalEndPoint { get; set; }
        public string LocalSoftwareIdentifier { get; set; }
        public string RemoteEndPoint { get; set; }
        public string RemoteSoftwareIdentifier { get; set; }
        public bool IsActive { get; set; }
        public DateTimeOffset CreatedUtc { get; set; }
    }
}
