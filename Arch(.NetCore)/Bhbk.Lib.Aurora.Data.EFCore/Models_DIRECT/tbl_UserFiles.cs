using System;
using System.Collections.Generic;

namespace Bhbk.Lib.Aurora.Data.EFCore.Models_DIRECT
{
    public partial class tbl_UserFiles
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid? FolderId { get; set; }
        public string VirtualName { get; set; }
        public string RealPath { get; set; }
        public string RealFileName { get; set; }
        public long FileSize { get; set; }
        public string FileHashSHA256 { get; set; }
        public bool FileReadOnly { get; set; }
        public DateTime Created { get; set; }
        public DateTime? LastAccessed { get; set; }
        public DateTime? LastUpdated { get; set; }

        public virtual tbl_UserFolders Folder { get; set; }
        public virtual tbl_Users User { get; set; }
    }
}
