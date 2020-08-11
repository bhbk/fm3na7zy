using System;
using System.IO;

namespace Bhbk.Daemon.Aurora.SFTP.FileSystems
{
    internal static class SmbFileSystemCommon
    {
        internal static DirectoryInfo ConvertPathToCifsFolder(string path)
        {
            path = path.Replace("/", @"\");

            return new DirectoryInfo(path);
        }

        internal static FileInfo ConvertPathToCifsFile(string path)
        {
            path = path.Replace("/", @"\");

            return new FileInfo(path);
        }
    }
}
