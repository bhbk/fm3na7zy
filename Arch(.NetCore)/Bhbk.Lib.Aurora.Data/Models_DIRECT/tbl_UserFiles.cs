using System;
using System.Collections.Generic;

namespace Bhbk.Lib.Aurora.Data.Models_DIRECT
{
    public partial class tbl_UserFiles
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid? VirtualParentId { get; set; }
        public string VirtualFileName { get; set; }
        public string RealFolder { get; set; }
        public string RealFileName { get; set; }
        public int FileSize { get; set; }
        public string FileHashSHA256 { get; set; }
        public DateTime Created { get; set; }
        public DateTime? LastAccessed { get; set; }
        public DateTime? LastUpdated { get; set; }

        public virtual tbl_Users User { get; set; }
    }
}
