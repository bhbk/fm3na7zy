using Rebex.IO.FileSystem;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Bhbk.Daemon.Aurora.SFTP.FileSystems
{
    class MemoryReadOnlyFileSystem : ReadOnlyFileSystemProvider
    {
        protected override bool Exists(NodePath path, NodeType nodeType)
        {
            throw new NotImplementedException();
        }

        protected override NodeBase GetChild(string name, DirectoryNode parent)
        {
            throw new NotImplementedException();
        }

        protected override IEnumerable<NodeBase> GetChildren(DirectoryNode parent, NodeType nodeType)
        {
            throw new NotImplementedException();
        }

        protected override NodeContent GetContent(NodeBase node, NodeContentParameters contentParameters)
        {
            throw new NotImplementedException();
        }

        protected override long GetLength(NodeBase node)
        {
            throw new NotImplementedException();
        }

        protected override NodeTimeInfo GetTimeInfo(NodeBase node)
        {
            throw new NotImplementedException();
        }
    }
}
