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
    
    public partial class E_Usage
    {
        public System.Guid UserId { get; set; }
        public string UserName { get; set; }
        public long QuotaInBytes { get; set; }
        public long QuotaUsedInBytes { get; set; }
        public short SessionMax { get; set; }
        public short SessionsInUse { get; set; }
    
        public virtual E_Login Login { get; set; }
    }
}
