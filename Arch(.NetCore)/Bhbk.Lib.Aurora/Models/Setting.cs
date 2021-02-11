using System;
using System.Collections.Generic;
using System.Text;

namespace Bhbk.Lib.Aurora.Models
{
    public class Setting
    {
        public System.Guid Id { get; set; }
        public Nullable<System.Guid> UserId { get; set; }
        public string ConfigKey { get; set; }
        public string ConfigValue { get; set; }
        public System.DateTimeOffset CreatedUtc { get; set; }
        public bool IsDeletable { get; set; }

        public virtual Login User { get; set; }
    }
}
