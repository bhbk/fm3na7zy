//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Bhbk.Lib.Aurora.Data_EF6.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Setting_EF
    {
        public System.Guid Id { get; set; }
        public Nullable<System.Guid> UserId { get; set; }
        public string ConfigKey { get; set; }
        public string ConfigValue { get; set; }
        public bool IsDeletable { get; set; }
        public System.DateTimeOffset CreatedUtc { get; set; }
    
        public virtual Login_EF Login { get; set; }
    }
}
