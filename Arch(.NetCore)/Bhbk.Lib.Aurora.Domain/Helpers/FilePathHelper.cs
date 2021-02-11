using System;
using System.IO;
using System.Text.RegularExpressions;

namespace Bhbk.Lib.Aurora.Domain.Helpers
{
    public static class FilePathHelper
    {
        public static DirectoryInfo PathToFolder(string path)
            => new DirectoryInfo(path.Replace("/", @"\"));

        public static FileInfo PathToFile(string path)
            => new FileInfo(path.Replace("/", @"\"));

        public static bool IsValidUncPath(string path)
        {
            return Uri.TryCreate(path, UriKind.Absolute, out Uri uri)
                && uri.IsUnc;
        }

        public static bool IsValidPosixPath(string path)
        {
            /*
             * https://stackoverflow.com/questions/6416065/c-sharp-regex-for-file-paths-e-g-c-test-test-exe/42036026#42036026
             * ^\/$|(^(?=\/)|^\.|^\.\.)(\/(?=[^\/\0])[^\/\0]+)*\/?$
             */
            var reg = new Regex($"^\\/$|(^(?=\\/)|^\\.|^\\.\\.)(\\/(?=[^\\/\0])[^\\/\0]+)*\\/?$",
                RegexOptions.IgnoreCase | RegexOptions.Compiled);

            return reg.IsMatch(path);
        }
    }
}
