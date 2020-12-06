using Bhbk.Lib.Aurora.Data.ModelsMem;
using Bhbk.Lib.Aurora.Data.UnitOfWorkMem;
using Bhbk.Lib.Aurora.Data_EF6.Models;
using Bhbk.Lib.QueryExpression.Extensions;
using Bhbk.Lib.QueryExpression.Factories;
using Serilog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;

namespace Bhbk.Daemon.Aurora.SFTP.Factories
{
    internal class MemoryPathFactory
    {
        internal static UserMem CheckContent(IUnitOfWorkMem uow, UserMem user)
        {
            /*
             * only neeed if in-memory sqlite instance is backed by entity framework 6. not needed if backed by ef core.
             */

            uow.UserFiles.Delete(QueryExpressionFactory.GetQueryExpression<UserFileMem>()
                .Where(x => x.IdentityId == user.IdentityId).ToLambda());

            uow.UserFolders.Delete(QueryExpressionFactory.GetQueryExpression<UserFolderMem>()
                .Where(x => x.IdentityId == user.IdentityId).ToLambda());

            uow.Commit();

            return user;
        }

        internal static UserFolderMem CheckFolder(IUnitOfWorkMem uow, UserMem user)
        {
            var callPath = $"{MethodBase.GetCurrentMethod().DeclaringType.Name}.{MethodBase.GetCurrentMethod().Name}";

            var folderMem = uow.UserFolders.Get(QueryExpressionFactory.GetQueryExpression<UserFolderMem>()
                .Where(x => x.IdentityId == user.IdentityId && x.ParentId == null).ToLambda())
                .SingleOrDefault();

            if (folderMem == null)
            {
                folderMem = uow.UserFolders.Create(
                    new UserFolderMem
                    {
                        Id = Guid.NewGuid(),
                        IdentityId = user.IdentityId,
                        ParentId = null,
                        VirtualName = string.Empty,
                        CreatedUtc = DateTime.UtcNow,
                        IsReadOnly = true,
                    });
                uow.Commit();

                Log.Information($"'{callPath}' '{user.IdentityAlias}' folder:'/' at:memory");
            }

            return folderMem;
        }

        internal static UserMem CheckUser(IUnitOfWorkMem uow, User user)
        {
            var callPath = $"{MethodBase.GetCurrentMethod().DeclaringType.Name}.{MethodBase.GetCurrentMethod().Name}";

            var userMem = uow.Users.Get(QueryExpressionFactory.GetQueryExpression<UserMem>()
                .Where(x => x.IdentityId == user.IdentityId).ToLambda())
                .SingleOrDefault();

            if (userMem == null)
            {
                userMem = uow.Users.Create(
                    new UserMem
                    {
                        IdentityId = user.IdentityId,
                        IdentityAlias = user.IdentityAlias,
                    });
                uow.Commit();

                Log.Information($"'{callPath}' '{user.IdentityAlias}' exists at:memory");
            }

            return userMem;
        }

        internal static UserFileMem PathToFile(IUnitOfWorkMem uow, UserMem user, string path)
        {
            if (path.FirstOrDefault() == '/')
                path = path.Substring(1);

            var pathBits = path.Split("/");
            var filePath = path.Split("/").Last();
            var folderPath = string.Empty;

            for (int i = 0; i <= pathBits.Count() - 2; i++)
                folderPath += "/" + pathBits.ElementAt(i);

            var folder = PathToFolder(uow, user, folderPath);

            var file = uow.UserFiles.Get(QueryExpressionFactory.GetQueryExpression<UserFileMem>()
                .Where(x => x.IdentityId == user.IdentityId && x.FolderId == folder.Id && x.VirtualName == filePath).ToLambda())
                .SingleOrDefault();

            return file;
        }

        internal static UserFolderMem PathToFolder(IUnitOfWorkMem uow, UserMem user, string path)
        {
            if (path.FirstOrDefault() == '/')
                path = path.Substring(1);

            var folder = uow.UserFolders.Get(QueryExpressionFactory.GetQueryExpression<UserFolderMem>()
                .Where(x => x.IdentityId == user.IdentityId && x.ParentId == null).ToLambda())
                .SingleOrDefault();

            if (string.IsNullOrWhiteSpace(path))
                return folder;

            foreach (var entry in path.Split("/"))
            {
                folder = uow.UserFolders.Get(QueryExpressionFactory.GetQueryExpression<UserFolderMem>()
                    .Where(x => x.IdentityId == user.IdentityId && x.ParentId == folder.Id && x.VirtualName == entry).ToLambda())
                    .SingleOrDefault();
            };

            return folder;
        }

        internal static string FileToPath(IUnitOfWorkMem uow, UserMem user, UserFileMem file)
        {
            var path = string.Empty;
            var paths = new List<string> { };

            var folder = uow.UserFolders.Get(QueryExpressionFactory.GetQueryExpression<UserFolderMem>()
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

        internal static string FolderToPath(IUnitOfWorkMem uow, UserMem user, UserFolderMem folder)
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
