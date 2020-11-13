using System;
using System.Collections.Generic;

#nullable disable

namespace Bhbk.Lib.Aurora.Data.Models
{
    public partial class uvw_Credential
    {
        public Guid Id { get; set; }
        public string Domain { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public bool IsEnabled { get; set; }
        public bool IsDeletable { get; set; }
        public DateTimeOffset CreatedUtc { get; set; }
        public DateTimeOffset? LastUpdatedUtc { get; set; }
    }
}
