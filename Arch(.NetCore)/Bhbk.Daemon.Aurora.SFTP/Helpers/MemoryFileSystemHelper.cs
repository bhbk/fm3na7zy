using Bhbk.Lib.Aurora.Data_EF6.Models;
using Rebex.IO.FileSystem;
using Serilog;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace Bhbk.Daemon.Aurora.SFTP.Helpers
{
    internal class MemoryFileSystemHelper
    {
        internal static void EnsureRootExists(DirectoryNode root, 
            Dictionary<NodePath, NodeBase> path, 
            Dictionary<NodeBase, MemoryNodeData> store)
        {
            if (store.Count == 0)
            {
                store.Add(root, new MemoryNodeData());
                path.Add(root.Path, root);
            }
        }

        internal static void CreatePubKeysFile(DirectoryNode root,
            Dictionary<NodePath, NodeBase> path,
            Dictionary<NodeBase, MemoryNodeData> store,
            User user,
            StringBuilder content)
        {
            var callPath = $"{MethodBase.GetCurrentMethod().DeclaringType.Name}.{MethodBase.GetCurrentMethod().Name}";

            var folderName = ".ssh";
            var folderNode = new DirectoryNode(folderName, root);
            var fileName = "authorized_users";
            var fileNode = new FileNode(fileName, folderNode);

            store.Add(folderNode, new MemoryNodeData());
            store[root].Children.Add(folderNode);
            path.Add(folderNode.Path, folderNode);

            store.Add(fileNode, 
                new MemoryNodeData()
                {
                    Content = new MemoryStream(Encoding.UTF8.GetBytes(content.ToString()))
                });
            store[folderNode].Children.Add(fileNode);
            path.Add(fileNode.Path, fileNode);

            Log.Information($"'{callPath}' '{user.IdentityAlias}' file '{fileNode.Path}'");
        }
    }

    internal class MemoryNodeData
    {
        internal MemoryNodeData()
        {
            Content = new MemoryStream();
            TimeInfo = new NodeTimeInfo();
            Children = new List<NodeBase>();
            Attributes = new NodeAttributes(FileAttributes.Offline);
        }

        internal NodeAttributes Attributes
        {
            get;
            set;
        }

        internal NodeTimeInfo TimeInfo
        {
            get;
            set;
        }

        internal List<NodeBase> Children
        {
            get;
            set;
        }

        internal long Length
        {
            get
            {
                return Content.Length;
            }
        }

        internal MemoryStream Content
        {
            get;
            set;
        }
    }
}
