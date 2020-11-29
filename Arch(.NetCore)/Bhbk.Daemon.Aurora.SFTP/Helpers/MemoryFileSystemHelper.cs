using Bhbk.Lib.Aurora.Data_EF6.Models;
using Rebex.IO.FileSystem;
using Serilog;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Bhbk.Daemon.Aurora.SFTP.Helpers;
using Bhbk.Lib.Aurora.Data_EF6.Models;
using Bhbk.Lib.Aurora.Domain.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Rebex.IO.FileSystem;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Bhbk.Daemon.Aurora.SFTP.Helpers
{
    internal class MemoryFileSystemHelper
    {
        internal static DirectoryNode EnsureRootExists(DirectoryNode root, 
            Dictionary<NodePath, NodeBase> path, 
            Dictionary<NodeBase, MemoryNodeData> store,
            User user)
        {
            if (store.Count == 0)
            {
                var callPath = $"{MethodBase.GetCurrentMethod().DeclaringType.Name}.{MethodBase.GetCurrentMethod().Name}";

                store.Add(root, new MemoryNodeData());
                path.Add(root.Path, root);

                Log.Information($"'{callPath}' '{user.IdentityAlias}' folder '/'");
            }

            return root;
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
