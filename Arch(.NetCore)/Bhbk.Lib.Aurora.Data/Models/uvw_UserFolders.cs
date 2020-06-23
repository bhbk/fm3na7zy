﻿using System;
using System.Collections.Generic;

namespace Bhbk.Lib.Aurora.Data.Models
{
    public partial class uvw_UserFolders
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid? ParentId { get; set; }
        public string VirtualName { get; set; }
        public DateTime Created { get; set; }
        public DateTime? LastAccessed { get; set; }
        public DateTime? LastUpdated { get; set; }
        public bool ReadOnly { get; set; }
    }
}
