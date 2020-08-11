using System;
using System.IO;

namespace Bhbk.Daemon.Aurora.SFTP.Helpers
{
    internal static class SmbFileSystemHelper
    {
        internal static DirectoryInfo FolderPathToCIFS(string path)
        {
            path = path.Replace("/", @"\");

            return new DirectoryInfo(path);
        }

        internal static FileInfo FilePathToCIFS(string path)
        {
            path = path.Replace("/", @"\");

            return new FileInfo(path);
        }
    }
}
