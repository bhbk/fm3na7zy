using System;
using System.Collections.Generic;

namespace Bhbk.Lib.Aurora.Data.Models_DIRECT
{
    public partial class tbl_Setting
    {
        public Guid Id { get; set; }
        public Guid? IdentityId { get; set; }
        public string ConfigKey { get; set; }
        public string ConfigValue { get; set; }
        public bool Deletable { get; set; }
        public DateTime Created { get; set; }
        public DateTime? LastUpdated { get; set; }

        public virtual tbl_User Identity { get; set; }
    }
}
