using System;
using System.IO;

namespace Bhbk.Daemon.Aurora.SSH.Helpers
{
    public static class SmbFileSystemHelper
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
