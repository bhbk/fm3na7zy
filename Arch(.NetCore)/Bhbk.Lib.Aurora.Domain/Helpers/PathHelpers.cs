using Bhbk.Lib.Aurora.Data.Models_DIRECT;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Reflection;

namespace Bhbk.Lib.Aurora.Domain.Helpers
{
    public class PathHelpers
    {
        public static DirectoryInfo GetUserRoot(IConfiguration conf, tbl_Users user)
        {
            var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
                + Path.DirectorySeparatorChar + "data"
                + Path.DirectorySeparatorChar + user.UserName;

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            return new DirectoryInfo(path);
        }
    }
}
