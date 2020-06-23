using System;
using System.Collections.Generic;

namespace Bhbk.Lib.Aurora.Data.Models_DIRECT
{
    public partial class tbl_SysSettings
    {
        public Guid Id { get; set; }
        public string ConfigKey { get; set; }
        public string ConfigValue { get; set; }
        public DateTime Created { get; set; }
        public bool Immutable { get; set; }
    }
}
