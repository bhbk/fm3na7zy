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
    
    public partial class usp_FileSystemLogin_Insert_Result
    {
        public System.Guid FileSystemId { get; set; }
        public System.Guid UserId { get; set; }
        public Nullable<int> SmbAuthTypeId { get; set; }
        public Nullable<System.Guid> AmbassadorId { get; set; }
        public string ChrootPath { get; set; }
        public System.DateTimeOffset CreatedUtc { get; set; }
        public bool IsReadOnly { get; set; }
    }
}
