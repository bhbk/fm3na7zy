using Bhbk.Lib.Aurora.Data.Infrastructure_DIRECT;
using Bhbk.Lib.Aurora.Data.Models_DIRECT;
using Bhbk.Lib.Common.Primitives;
using Bhbk.Lib.QueryExpression.Extensions;
using Bhbk.Lib.QueryExpression.Factories;
using Microsoft.Extensions.Configuration;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;

namespace Bhbk.Daemon.Aurora.SFTP.Helpers
{
    internal class CompositeFileSystemHelper
    {
        internal static void EnsureRootExists(IUnitOfWork uow, tbl_Users user)
        {
            var folder = uow.UserFolders.Get(QueryExpressionFactory.GetQueryExpression<tbl_UserFolders>()
                .Where(x => x.UserId == user.Id && x.ParentId == null).ToLambda())
                .SingleOrDefault();

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

        internal static tbl_UserFiles FilePathToEntity(IUnitOfWork uow, tbl_Users user, string path)
        {
            if (path.FirstOrDefault() == '/')
                path = path.Substring(1);

            var pathBits = path.Split("/");
            var filePath = path.Split("/").Last();
            var folderPath = string.Empty;

            for (int i = 0; i <= pathBits.Count() - 2; i++)
                folderPath += "/" + pathBits.ElementAt(i);

            var folder = FolderPathToEntity(uow, user, folderPath);

            var file = uow.UserFiles.Get(QueryExpressionFactory.GetQueryExpression<tbl_UserFiles>()
                .Where(x => x.UserId == user.Id && x.FolderId == folder.Id && x.VirtualName == filePath).ToLambda())
                .SingleOrDefault();

            return file;
        }

        internal static tbl_UserFolders FolderPathToEntity(IUnitOfWork uow, tbl_Users user, string path)
        {
            if (path.FirstOrDefault() == '/')
                path = path.Substring(1);

            var folder = uow.UserFolders.Get(QueryExpressionFactory.GetQueryExpression<tbl_UserFolders>()
                .Where(x => x.UserId == user.Id && x.ParentId == null).ToLambda())
                .SingleOrDefault();

            if (string.IsNullOrWhiteSpace(path))
                return folder;

            foreach (var entry in path.Split("/"))
            {
                folder = uow.UserFolders.Get(QueryExpressionFactory.GetQueryExpression<tbl_UserFolders>()
                    .Where(x => x.UserId == user.Id && x.ParentId == folder.Id && x.VirtualName == entry).ToLambda())
                    .SingleOrDefault();
            };

            return folder;
        }

        internal static string FileEntityToPath(IUnitOfWork uow, tbl_Users user, tbl_UserFiles file)
        {
            var path = string.Empty;
            var paths = new List<string> { };

            var folder = uow.UserFolders.Get(QueryExpressionFactory.GetQueryExpression<tbl_UserFolders>()
                .Where(x => x.UserId == user.Id && x.Id == file.FolderId).ToLambda())
                .Single();

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

        internal static string FolderEntityToPath(IUnitOfWork uow, tbl_Users user, tbl_UserFolders folder)
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

        internal static tbl_UserFiles SaveFileStream(IConfiguration conf, Stream content, tbl_UserFiles fileEntity)
        {
            var folder = new DirectoryInfo(conf["Storage:BaseLocalPath"]
                + Path.DirectorySeparatorChar + fileEntity.RealPath);

            if (!folder.Exists)
                folder.Create();

            var file = new FileInfo(conf["Storage:BaseLocalPath"]
                + Path.DirectorySeparatorChar + fileEntity.RealPath
                + Path.DirectorySeparatorChar + fileEntity.RealFileName);

            using (var fs = new FileStream(file.FullName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite))
                content.CopyTo(fs);

            using (var sha256 = new SHA256Managed())
            using (var fs = new FileStream(file.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                var hash = sha256.ComputeHash(fs);

                fileEntity.RealFileSize = fs.Length;
                fileEntity.HashSHA256 = Strings.GetHexString(hash);
                fileEntity.ReadOnly = false;
                fileEntity.Created = DateTime.UtcNow;
                fileEntity.LastAccessed = null;
                fileEntity.LastUpdated = null;
            }

            return fileEntity;
        }
    }
}
