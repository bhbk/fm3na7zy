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
    
    public partial class usp_UserFile_Delete_Result
    {
        public System.Guid Id { get; set; }
        public System.Guid IdentityId { get; set; }
        public Nullable<System.Guid> FolderId { get; set; }
        public string VirtualName { get; set; }
        public string RealPath { get; set; }
        public string RealFileName { get; set; }
        public long RealFileSize { get; set; }
        public string HashSHA256 { get; set; }
        public bool IsReadOnly { get; set; }
        public System.DateTimeOffset CreatedUtc { get; set; }
        public Nullable<System.DateTimeOffset> LastAccessedUtc { get; set; }
        public Nullable<System.DateTimeOffset> LastUpdatedUtc { get; set; }
        public System.DateTimeOffset LastVerifiedUtc { get; set; }
    }
}