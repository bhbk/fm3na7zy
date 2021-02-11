using System;
using System.Collections.Generic;
using System.Text;

namespace Bhbk.Lib.Aurora.Models
{
    public class LoginAuthType
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsEnabled { get; set; }
        public bool IsEditable { get; set; }
        public bool IsDeletable { get; set; }

        public virtual ICollection<Login> Users { get; set; }
    }
}
