﻿using System;
using System.Collections.Generic;

namespace Bhbk.Lib.Aurora.Data.Models_DIRECT
{
    public partial class tbl_UserFile
    {
        public Guid Id { get; set; }
        public Guid IdentityId { get; set; }
        public Guid? FolderId { get; set; }
        public string VirtualName { get; set; }
        public bool ReadOnly { get; set; }
        public string RealPath { get; set; }
        public string RealFileName { get; set; }
        public long RealFileSize { get; set; }
        public string HashSHA256 { get; set; }
        public DateTime Created { get; set; }
        public DateTime? LastAccessed { get; set; }
        public DateTime? LastUpdated { get; set; }
        public DateTime LastVerified { get; set; }

        public virtual tbl_UserFolder Folder { get; set; }
        public virtual tbl_User Identity { get; set; }
    }
}
