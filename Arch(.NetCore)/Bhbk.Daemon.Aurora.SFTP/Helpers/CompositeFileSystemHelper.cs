using Bhbk.Lib.Aurora.Data_EF6.Infrastructure;
using Bhbk.Lib.Aurora.Data_EF6.Models;
using Bhbk.Lib.Common.Primitives;
using Bhbk.Lib.Identity.Services;
using Bhbk.Lib.QueryExpression.Extensions;
using Bhbk.Lib.QueryExpression.Factories;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Rebex.IO.FileSystem;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace Bhbk.Daemon.Aurora.SFTP.Helpers
{
    internal class CompositeFileSystemHelper
    {
        internal static void EnsureRootExists(IUnitOfWork uow, User user)
        {
            var folder = uow.UserFolders.Get(QueryExpressionFactory.GetQueryExpression<UserFolder>()
                .Where(x => x.IdentityId == user.IdentityId && x.ParentId == null).ToLambda())
                .SingleOrDefault();

            if (folder == null)
            {
                var now = DateTime.UtcNow;

                var newFolder = uow.UserFolders.Create(
                    new UserFolder
                    {
                        Id = Guid.NewGuid(),
                        IdentityId = user.IdentityId,
                        ParentId = null,
                        VirtualName = string.Empty,
                        CreatedUtc = now,
                        IsReadOnly = true,
                    });
                uow.Commit();

                var callPath = $"{MethodBase.GetCurrentMethod().DeclaringType.Name}.{MethodBase.GetCurrentMethod().Name}";

                Log.Information($"'{callPath}' '{user.IdentityAlias}' initialize '/'");
            }
        }

        internal static UserFile FilePathToEntity(IUnitOfWork uow, User user, string path)
        {
            if (path.FirstOrDefault() == '/')
                path = path.Substring(1);

            var pathBits = path.Split("/");
            var filePath = path.Split("/").Last();
            var folderPath = string.Empty;

            for (int i = 0; i <= pathBits.Count() - 2; i++)
                folderPath += "/" + pathBits.ElementAt(i);

            var folder = FolderPathToEntity(uow, user, folderPath);

            var file = uow.UserFiles.Get(QueryExpressionFactory.GetQueryExpression<UserFile>()
                .Where(x => x.IdentityId == user.IdentityId && x.FolderId == folder.Id && x.VirtualName == filePath).ToLambda())
                .SingleOrDefault();

            return file;
        }

        internal static UserFolder FolderPathToEntity(IUnitOfWork uow, User user, string path)
        {
            if (path.FirstOrDefault() == '/')
                path = path.Substring(1);

            var folder = uow.UserFolders.Get(QueryExpressionFactory.GetQueryExpression<UserFolder>()
                .Where(x => x.IdentityId == user.IdentityId && x.ParentId == null).ToLambda())
                .SingleOrDefault();

            if (string.IsNullOrWhiteSpace(path))
                return folder;

            foreach (var entry in path.Split("/"))
            {
                folder = uow.UserFolders.Get(QueryExpressionFactory.GetQueryExpression<UserFolder>()
                    .Where(x => x.IdentityId == user.IdentityId && x.ParentId == folder.Id && x.VirtualName == entry).ToLambda())
                    .SingleOrDefault();
            };

            return folder;
        }

        internal static string FileEntityToPath(IUnitOfWork uow, User user, UserFile file)
        {
            var path = string.Empty;
            var paths = new List<string> { };

            var folder = uow.UserFolders.Get(QueryExpressionFactory.GetQueryExpression<UserFolder>()
                .Where(x => x.IdentityId == user.IdentityId && x.Id == file.FolderId).ToLambda())
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

        internal static string FolderEntityToPath(IUnitOfWork uow, User user, UserFolder folder)
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

        internal static UserFile SaveFileStream(IConfiguration conf, Stream content, UserFile fileEntity)
        {
            var folder = new DirectoryInfo(conf["Storage:UnstructuredData"]
                + Path.DirectorySeparatorChar + fileEntity.RealPath);

            if (!folder.Exists)
                folder.Create();

            var file = new FileInfo(conf["Storage:UnstructuredData"]
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
                fileEntity.IsReadOnly = false;
                fileEntity.CreatedUtc = DateTime.UtcNow;
            }

            return fileEntity;
        }
    }
}
