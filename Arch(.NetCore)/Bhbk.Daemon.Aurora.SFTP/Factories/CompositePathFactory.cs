using Bhbk.Lib.Aurora.Data_EF6.UnitOfWork;
using Bhbk.Lib.Aurora.Data_EF6.Models;
using Bhbk.Lib.QueryExpression.Extensions;
using Bhbk.Lib.QueryExpression.Factories;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Bhbk.Daemon.Aurora.SFTP.Factories
{
    internal class CompositePathFactory
    {
        internal static UserFolder CheckFolderRoot(IUnitOfWork uow, User user)
        {
            var callPath = $"{MethodBase.GetCurrentMethod().DeclaringType.Name}.{MethodBase.GetCurrentMethod().Name}";

            var folder = uow.UserFolders.Get(QueryExpressionFactory.GetQueryExpression<UserFolder>()
                .Where(x => x.IdentityId == user.IdentityId && x.ParentId == null).ToLambda())
                .SingleOrDefault();
            
            if (folder == null)
            {
                folder = uow.UserFolders.Create(
                    new UserFolder
                    {
                        IdentityId = user.IdentityId,
                        ParentId = null,
                        VirtualName = string.Empty,
                        CreatedUtc = DateTime.UtcNow,
                        IsReadOnly = true,
                    });
                uow.Commit();

                Log.Information($"'{callPath}' '{user.IdentityAlias}' folder '/'");
            }

            return folder;
        }

        internal static UserFile PathToFile(IUnitOfWork uow, User user, string path)
        {
            if (path.FirstOrDefault() == '/')
                path = path.Substring(1);

            var pathBits = path.Split("/");
            var filePath = path.Split("/").Last();
            var folderPath = string.Empty;

            for (int i = 0; i <= pathBits.Count() - 2; i++)
                folderPath += "/" + pathBits.ElementAt(i);

            var folder = PathToFolder(uow, user, folderPath);

            var file = uow.UserFiles.Get(QueryExpressionFactory.GetQueryExpression<UserFile>()
                .Where(x => x.IdentityId == user.IdentityId && x.FolderId == folder.Id && x.VirtualName == filePath).ToLambda())
                .SingleOrDefault();

            return file;
        }

        internal static UserFolder PathToFolder(IUnitOfWork uow, User user, string path)
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

        internal static string FileToPath(IUnitOfWork uow, User user, UserFile file)
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

        internal static string FolderToPath(IUnitOfWork uow, User user, UserFolder folder)
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
    }
}
