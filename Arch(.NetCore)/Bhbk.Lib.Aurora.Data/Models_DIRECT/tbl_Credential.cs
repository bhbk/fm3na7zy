using System;
using System.Collections.Generic;

#nullable disable

namespace Bhbk.Lib.Aurora.Data.Models_DIRECT
{
    public partial class tbl_Credential
    {
        public tbl_Credential()
        {
            tbl_UserMounts = new HashSet<tbl_UserMount>();
        }

        public Guid Id { get; set; }
        public string Domain { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public bool IsEnabled { get; set; }
        public bool IsDeletable { get; set; }
        public DateTimeOffset CreatedUtc { get; set; }
        public DateTimeOffset? LastUpdatedUtc { get; set; }

        public virtual ICollection<tbl_UserMount> tbl_UserMounts { get; set; }
    }
}
