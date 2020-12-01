﻿using System;
using System.Collections.Generic;

#nullable disable

namespace Bhbk.Lib.Aurora.Data.ModelsMem
{
    public partial class UserMem
    {
        public UserMem()
        {
            Files = new HashSet<UserFileMem>();
            Folders = new HashSet<UserFolderMem>();
        }

        public Guid IdentityId { get; set; }
        public string IdentityAlias { get; set; }
        public virtual ICollection<UserFileMem> Files { get; set; }
        public virtual ICollection<UserFolderMem> Folders { get; set; }
    }
}