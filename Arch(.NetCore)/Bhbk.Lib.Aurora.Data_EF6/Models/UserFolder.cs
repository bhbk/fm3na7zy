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
    
    public partial class UserFolder
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public UserFolder()
        {
            this.Files = new HashSet<UserFile>();
            this.Folders = new HashSet<UserFolder>();
        }
    
        public System.Guid Id { get; set; }
        public System.Guid IdentityId { get; set; }
        public Nullable<System.Guid> ParentId { get; set; }
        public string VirtualName { get; set; }
        public bool IsReadOnly { get; set; }
        public System.DateTimeOffset CreatedUtc { get; set; }
        public Nullable<System.DateTimeOffset> LastAccessedUtc { get; set; }
        public Nullable<System.DateTimeOffset> LastUpdatedUtc { get; set; }
    
        public virtual User User { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<UserFile> Files { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<UserFolder> Folders { get; set; }
        public virtual UserFolder Parent { get; set; }
    }
}