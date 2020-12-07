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
        internal static UserLoginMem CheckContent(IUnitOfWorkMem uow, UserLoginMem userMem)
        {
            /*
             * only neeed if in-memory sqlite instance is backed by entity framework 6. not needed if backed by ef core.
             */

            uow.UserFiles.Delete(QueryExpressionFactory.GetQueryExpression<UserFileMem>()
                .Where(x => x.IdentityId == userMem.IdentityId).ToLambda());

            uow.UserFolders.Delete(QueryExpressionFactory.GetQueryExpression<UserFolderMem>()
                .Where(x => x.IdentityId == userMem.IdentityId).ToLambda());

            uow.Commit();

            return userMem;
        }

        internal static UserFolderMem CheckFolder(IUnitOfWorkMem uow, UserLoginMem userMem)
        {
            var callPath = $"{MethodBase.GetCurrentMethod().DeclaringType.Name}.{MethodBase.GetCurrentMethod().Name}";

            var folderMem = uow.UserFolders.Get(QueryExpressionFactory.GetQueryExpression<UserFolderMem>()
                .Where(x => x.IdentityId == userMem.IdentityId && x.ParentId == null).ToLambda())
                .SingleOrDefault();

            if (folderMem == null)
            {
                folderMem = uow.UserFolders.Create(
                    new UserFolderMem
                    {
                        Id = Guid.NewGuid(),
                        IdentityId = userMem.IdentityId,
                        ParentId = null,
                        VirtualName = string.Empty,
                        CreatedUtc = DateTime.UtcNow,
                        IsReadOnly = true,
                    });
                uow.Commit();

                Log.Information($"'{callPath}' '{userMem.IdentityAlias}' folder:'/' at:memory");
            }

            return folderMem;
        }

        internal static UserLoginMem CheckUser(IUnitOfWorkMem uow, UserLogin user)
        {
            var callPath = $"{MethodBase.GetCurrentMethod().DeclaringType.Name}.{MethodBase.GetCurrentMethod().Name}";

            var userMem = uow.UserLogins.Get(QueryExpressionFactory.GetQueryExpression<UserLoginMem>()
                .Where(x => x.IdentityId == user.IdentityId).ToLambda())
                .SingleOrDefault();

            if (userMem == null)
            {
                userMem = uow.UserLogins.Create(
                    new UserLoginMem
                    {
                        IdentityId = user.IdentityId,
                        IdentityAlias = user.IdentityAlias,
                    });
                uow.Commit();

                Log.Information($"'{callPath}' '{user.IdentityAlias}' exists at:memory");
            }

            return userMem;
        }

        internal static UserFileMem PathToFile(IUnitOfWorkMem uow, UserLoginMem userMem, string path)
        {
            if (path.FirstOrDefault() == '/')
                path = path.Substring(1);

            var pathBits = path.Split("/");
            var filePath = path.Split("/").Last();
            var folderPath = string.Empty;

            for (int i = 0; i <= pathBits.Count() - 2; i++)
                folderPath += "/" + pathBits.ElementAt(i);

            var folder = PathToFolder(uow, userMem, folderPath);

            var file = uow.UserFiles.Get(QueryExpressionFactory.GetQueryExpression<UserFileMem>()
                .Where(x => x.IdentityId == userMem.IdentityId && x.FolderId == folder.Id && x.VirtualName == filePath).ToLambda())
                .SingleOrDefault();

            return file;
        }

        internal static UserFolderMem PathToFolder(IUnitOfWorkMem uow, UserLoginMem userMem, string path)
        {
            if (path.FirstOrDefault() == '/')
                path = path.Substring(1);

            var folder = uow.UserFolders.Get(QueryExpressionFactory.GetQueryExpression<UserFolderMem>()
                .Where(x => x.IdentityId == userMem.IdentityId && x.ParentId == null).ToLambda())
                .SingleOrDefault();

            if (string.IsNullOrWhiteSpace(path))
                return folder;

            foreach (var entry in path.Split("/"))
            {
                folder = uow.UserFolders.Get(QueryExpressionFactory.GetQueryExpression<UserFolderMem>()
                    .Where(x => x.IdentityId == userMem.IdentityId && x.ParentId == folder.Id && x.VirtualName == entry).ToLambda())
                    .SingleOrDefault();
            };

            return folder;
        }

        internal static string FileToPath(IUnitOfWorkMem uow, UserLoginMem userMem, UserFileMem fileMem)
        {
            var path = string.Empty;
            var paths = new List<string> { };

            var folder = uow.UserFolders.Get(QueryExpressionFactory.GetQueryExpression<UserFolderMem>()
                .Where(x => x.IdentityId == userMem.IdentityId && x.Id == fileMem.FolderId).ToLambda())
                .Single();

            while (folder.ParentId != null)
            {
                paths.Add(folder.VirtualName);
                folder = folder.Parent;
            }

            for (int i = paths.Count() - 1; i >= 0; i--)
                path += "/" + paths.ElementAt(i);

            path += "/" + fileMem.VirtualName;

            return path;
        }

        internal static string FolderToPath(IUnitOfWorkMem uow, UserLoginMem userMem, UserFolderMem folderMem)
        {
            var path = string.Empty;
            var paths = new List<string> { };

            while (folderMem.ParentId != null)
            {
                paths.Add(folderMem.VirtualName);
                folderMem = folderMem.Parent;
            }

            for (int i = paths.Count() - 1; i >= 0; i--)
                path += "/" + paths.ElementAt(i);

            return path;
        }
    }
}
