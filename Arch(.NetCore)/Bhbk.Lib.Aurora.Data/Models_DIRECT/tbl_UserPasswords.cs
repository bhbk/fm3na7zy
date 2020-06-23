using System;
using System.Collections.Generic;

namespace Bhbk.Lib.Aurora.Data.Models_DIRECT
{
    public partial class tbl_UserPasswords
    {
        public Guid UserId { get; set; }
        public string ConcurrencyStamp { get; set; }
        public string HashPBKDF2 { get; set; }
        public string HashSHA256 { get; set; }
        public string SecurityStamp { get; set; }
        public bool Enabled { get; set; }
        public DateTime Created { get; set; }
        public bool Immutable { get; set; }

        public virtual tbl_Users User { get; set; }
    }
}
