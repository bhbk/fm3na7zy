using Bhbk.Lib.Aurora.Data.Infrastructure_DIRECT;
using Bhbk.Lib.Aurora.Data.Models_DIRECT;
using Bhbk.Lib.QueryExpression.Extensions;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Bhbk.Daemon.Aurora.SFTP.FileSystems
{
    internal class CompositeFileSystemResolver
    {
        internal static tbl_UserFolders ConvertPathToSqlFolder(IUnitOfWork uow, tbl_Users user, string path)
        {
            if (path.FirstOrDefault() == '/')
                path = path.Substring(1);

            var folder = uow.UserFolders.Get(x => x.UserId == user.Id
                && x.ParentId == null).SingleOrDefault();

            if (string.IsNullOrWhiteSpace(path))
                return folder;

            foreach (var entry in path.Split("/"))
            {
                folder = uow.UserFolders.Get(x => x.UserId == user.Id
                    && x.ParentId == folder.Id
                    && x.VirtualName == entry).SingleOrDefault();
            };

            return folder;
        }

        internal static tbl_UserFiles ConvertPathToSqlFile(IUnitOfWork uow, tbl_Users user, string path)
        {
            if (path.FirstOrDefault() == '/')
                path = path.Substring(1);

            var pathBits = path.Split("/");
            var filePath = path.Split("/").Last();
            var folderPath = string.Empty;

            for (int i = 0; i <= pathBits.Count() - 2; i++)
                folderPath += "/" + pathBits.ElementAt(i);

            var folder = ConvertPathToSqlFolder(uow, user, folderPath);

            var file = uow.UserFiles.Get(x => x.UserId == user.Id
                && x.FolderId == folder.Id
                && x.VirtualName == filePath).SingleOrDefault();

            return file;
        }

        internal static string ConvertSqlToPathFolder(IUnitOfWork uow, tbl_Users user, tbl_UserFolders folder)
        {
            var path = string.Empty;
            var paths = new List<string> { };

            while (folder.ParentId != null)
            {
                paths.Add(folder.VirtualName);
                folder = folder.Parent;
            }

            for (int i = paths.Count() - 1; i >= 0; i--)
                path += "/" + paths.ElementAt(i);

            return path;
        }

        internal static string ConvertSqlToPathFile(IUnitOfWork uow, tbl_Users user, tbl_UserFiles file)
        {
            var path = string.Empty;
            var paths = new List<string> { };

            var folder = uow.UserFolders.Get(x => x.UserId == user.Id
                && x.Id == file.FolderId).Single();

            while (folder.ParentId != null)
            {
                paths.Add(folder.VirtualName);
                folder = folder.Parent;
            }

            for (int i = paths.Count() - 1; i >= 0; i--)
                path += "/" + paths.ElementAt(i);

            path += "/" + file.VirtualName;

            return path;
        }

        internal static void EnsureRootFolderExists(IUnitOfWork uow, tbl_Users user)
        {
            var folder = uow.UserFolders.Get(x => x.UserId == user.Id
                && x.ParentId == null).SingleOrDefault();

            if (folder == null)
            {
                var now = DateTime.UtcNow;

                var newFolder = uow.UserFolders.Create(
                    new tbl_UserFolders
                    {
                        Id = Guid.NewGuid(),
                        UserId = user.Id,
                        ParentId = null,
                        VirtualName = string.Empty,
                        Created = now,
                        LastAccessed = null,
                        LastUpdated = null,
                        ReadOnly = true,
                    });
                uow.Commit();

                var callPath = $"{MethodBase.GetCurrentMethod().DeclaringType.Name}.{MethodBase.GetCurrentMethod().Name}";
                Log.Information($"'{callPath}' '{user.UserName}' initialize '/'");
            }
        }
    }
}
