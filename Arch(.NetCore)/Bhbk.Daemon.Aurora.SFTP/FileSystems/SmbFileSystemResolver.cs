using System;
using System.IO;

namespace Bhbk.Daemon.Aurora.SFTP.FileSystems
{
    public static class SmbFileSystemResolver
    {
        public static DirectoryInfo ConvertPathToCifsFolder(string path)
        {
            path = path.Replace("/", @"\");

            return new DirectoryInfo(path);
        }

        public static FileInfo ConvertPathToCifsFile(string path)
        {
            path = path.Replace("/", @"\");

            return new FileInfo(path);
        }
    }
}
