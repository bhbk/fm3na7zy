using System.IO;

namespace Bhbk.Daemon.Aurora.SFTP.Factories
{
    internal static class SmbPathFactory
    {
        internal static DirectoryInfo PathToFolder(string path)
        {
            path = path.Replace("/", @"\");

            return new DirectoryInfo(path);
        }

        internal static FileInfo PathToFile(string path)
        {
            path = path.Replace("/", @"\");

            return new FileInfo(path);
        }
    }
}
