using Rebex.IO.FileSystem;
using System.Collections.Generic;
using System.IO;

namespace Bhbk.Daemon.Aurora.SFTP.FileSystems
{
    internal class MemoryFileSystemCommon
    {

    }

    internal class MemoryNodeData
    {
        public MemoryNodeData()
        {
            Content = new MemoryStream();
            TimeInfo = new NodeTimeInfo();
            Children = new List<NodeBase>();
            Attributes = new NodeAttributes(FileAttributes.Offline);
        }

        public NodeAttributes Attributes
        {
            get;
            set;
        }

        public NodeTimeInfo TimeInfo
        {
            get;
            set;
        }

        public List<NodeBase> Children
        {
            get;
            set;
        }

        public long Length
        {
            get
            {
                return Content.Length;
            }
        }

        public MemoryStream Content
        {
            get;
            set;
        }
    }
}
