using System;
using System.Collections.Generic;

#nullable disable

namespace Bhbk.Lib.Aurora.Data.Models
{
    public partial class uvw_File
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid FolderId { get; set; }
        public string VirtualName { get; set; }
        public bool IsReadOnly { get; set; }
        public string RealPath { get; set; }
        public string RealFileName { get; set; }
        public long RealFileSize { get; set; }
        public string HashSHA256 { get; set; }
        public DateTimeOffset CreatedUtc { get; set; }
        public DateTimeOffset LastAccessedUtc { get; set; }
        public DateTimeOffset LastUpdatedUtc { get; set; }
        public DateTimeOffset LastVerifiedUtc { get; set; }
    }
}
