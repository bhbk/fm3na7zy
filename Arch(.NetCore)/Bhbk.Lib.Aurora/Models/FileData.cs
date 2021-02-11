namespace Bhbk.Lib.Aurora.Models
{
    public class FileData
    {
        public System.Guid Id { get; set; }
        public System.Guid FileSystemId { get; set; }
        public System.Guid FolderId { get; set; }
        public string VirtualName { get; set; }
        public bool IsReadOnly { get; set; }
        public string RealPath { get; set; }
        public string RealName { get; set; }
        public long RealSize { get; set; }
        public int HashTypeId { get; set; }
        public string HashValue { get; set; }
        public System.Guid CreatedByUserId { get; set; }
        public string CreatedByUserName { get; set; }
        public System.DateTimeOffset CreatedUtc { get; set; }
        public System.DateTimeOffset LastAccessedUtc { get; set; }
        public System.DateTimeOffset LastUpdatedUtc { get; set; }
        public System.DateTimeOffset LastVerifiedUtc { get; set; }

        public virtual Login Creator { get; set; }
        public virtual FileSystem FileSystem { get; set; }
        public virtual HashAlgorithmType HashAlgorithmType { get; set; }
        public virtual FolderData Parent { get; set; }
    }
}
