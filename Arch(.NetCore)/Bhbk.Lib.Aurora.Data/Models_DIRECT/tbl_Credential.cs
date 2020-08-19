using System;
using System.Collections.Generic;

namespace Bhbk.Lib.Aurora.Data.Models_DIRECT
{
    public partial class tbl_Credential
    {
        public tbl_Credential()
        {
            tbl_UserMount = new HashSet<tbl_UserMount>();
        }

        public Guid Id { get; set; }
        public string Domain { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public bool Enabled { get; set; }
        public bool Deletable { get; set; }
        public DateTime Created { get; set; }
        public DateTime? LastUpdated { get; set; }

        public virtual ICollection<tbl_UserMount> tbl_UserMount { get; set; }
    }
}
