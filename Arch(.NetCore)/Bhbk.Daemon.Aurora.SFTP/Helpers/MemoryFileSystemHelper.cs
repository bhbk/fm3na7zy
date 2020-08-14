using Bhbk.Lib.Identity.Services;
using Newtonsoft.Json;
using Rebex.IO.FileSystem;
using System;
using System.Collections.Generic;
using System.IO;
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

        internal static void GenerateContentForMOTD(DirectoryNode root, 
            Dictionary<NodePath, NodeBase> path, 
            Dictionary<NodeBase, MemoryNodeData> store, 
            IMeService me)
        {
            var msg = me.Info_GetMOTDV1().Result;

            var childFileName = "msg-" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + ".txt";
            var childNodeData = new MemoryNodeData()
            {
                Content = new MemoryStream(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(msg, Formatting.Indented)))
            };
            var childNode = new FileNode(childFileName, root);
            var childPath = new NodePath(root.Path + childFileName);

            store.Add(childNode, childNodeData);
            store[root].Children.Add(childNode);
            path.Add(childPath, childNode);
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
