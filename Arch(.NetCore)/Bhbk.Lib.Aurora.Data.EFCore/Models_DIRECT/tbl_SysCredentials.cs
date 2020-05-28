using System;
using System.Collections.Generic;

namespace Bhbk.Lib.Aurora.Data.EFCore.Models_DIRECT
{
    public partial class tbl_SysCredentials
    {
        public tbl_SysCredentials()
        {
            tbl_UserMounts = new HashSet<tbl_UserMounts>();
        }

        public Guid Id { get; set; }
        public string Domain { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public bool Enabled { get; set; }
        public DateTime Created { get; set; }
        public bool Immutable { get; set; }

        public virtual ICollection<tbl_UserMounts> tbl_UserMounts { get; set; }
    }
}
