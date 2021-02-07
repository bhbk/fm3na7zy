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
        internal static E_LoginMem CheckContent(IUnitOfWorkMem uow, E_LoginMem userMem)
        {
            /*
             * only neeed if in-memory sqlite env is backed by entity framework 6. not needed if backed by ef core.
             */

            uow.Files.Delete(QueryExpressionFactory.GetQueryExpression<E_FileMem>()
                .Where(x => x.UserId == userMem.UserId).ToLambda());

            uow.Folders.Delete(QueryExpressionFactory.GetQueryExpression<E_FolderMem>()
                .Where(x => x.UserId == userMem.UserId).ToLambda());

            uow.Commit();

            return userMem;
        }

        internal static E_FolderMem CheckFolder(IUnitOfWorkMem uow, E_LoginMem userMem)
        {
            var callPath = $"{MethodBase.GetCurrentMethod().DeclaringType.Name}.{MethodBase.GetCurrentMethod().Name}";

            var folderMem = uow.Folders.Get(QueryExpressionFactory.GetQueryExpression<E_FolderMem>()
                .Where(x => x.UserId == userMem.UserId && x.ParentId == null).ToLambda())
                .SingleOrDefault();

            if (folderMem == null)
            {
                folderMem = uow.Folders.Create(
                    new E_FolderMem
                    {
                        Id = Guid.NewGuid(),
                        UserId = userMem.UserId,
                        ParentId = null,
                        VirtualName = string.Empty,
                        CreatedUtc = DateTime.UtcNow,
                        IsReadOnly = true,
                    });
                uow.Commit();

                Log.Information($"'{callPath}' '{userMem.UserName}' folder:'/' at:memory");
            }

            return folderMem;
        }

        internal static E_LoginMem CheckUser(IUnitOfWorkMem uow, E_Login user)
        {
            var callPath = $"{MethodBase.GetCurrentMethod().DeclaringType.Name}.{MethodBase.GetCurrentMethod().Name}";

            var userMem = uow.Logins.Get(QueryExpressionFactory.GetQueryExpression<E_LoginMem>()
                .Where(x => x.UserId == user.UserId).ToLambda())
                .SingleOrDefault();

            if (userMem == null)
            {
                userMem = uow.Logins.Create(
                    new E_LoginMem
                    {
                        UserId = user.UserId,
                        UserName = user.UserName,
                    });
                uow.Commit();

                Log.Information($"'{callPath}' '{user.UserName}' exists at:memory");
            }

            return userMem;
        }

        internal static E_FileMem PathToFile(IUnitOfWorkMem uow, E_LoginMem userMem, string path)
        {
            if (path.FirstOrDefault() == '/')
                path = path.Substring(1);

            var pathBits = path.Split("/");
            var filePath = path.Split("/").Last();
            var folderPath = string.Empty;

            for (int i = 0; i <= pathBits.Count() - 2; i++)
                folderPath += "/" + pathBits.ElementAt(i);

            var folder = PathToFolder(uow, userMem, folderPath);

            var file = uow.Files.Get(QueryExpressionFactory.GetQueryExpression<E_FileMem>()
                .Where(x => x.UserId == userMem.UserId && x.FolderId == folder.Id && x.VirtualName == filePath).ToLambda())
                .SingleOrDefault();

            return file;
        }

        internal static E_FolderMem PathToFolder(IUnitOfWorkMem uow, E_LoginMem userMem, string path)
        {
            if (path.FirstOrDefault() == '/')
                path = path.Substring(1);

            var folder = uow.Folders.Get(QueryExpressionFactory.GetQueryExpression<E_FolderMem>()
                .Where(x => x.UserId == userMem.UserId && x.ParentId == null).ToLambda())
                .SingleOrDefault();

            if (string.IsNullOrWhiteSpace(path))
                return folder;

            foreach (var entry in path.Split("/"))
            {
                folder = uow.Folders.Get(QueryExpressionFactory.GetQueryExpression<E_FolderMem>()
                    .Where(x => x.UserId == userMem.UserId && x.ParentId == folder.Id && x.VirtualName == entry).ToLambda())
                    .SingleOrDefault();
            };

            return folder;
        }

        internal static string FileToPath(IUnitOfWorkMem uow, E_LoginMem userMem, E_FileMem fileMem)
        {
            var path = string.Empty;
            var paths = new List<string> { };

            var folder = uow.Folders.Get(QueryExpressionFactory.GetQueryExpression<E_FolderMem>()
                .Where(x => x.UserId == userMem.UserId && x.Id == fileMem.FolderId).ToLambda())
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

        internal static string FolderToPath(IUnitOfWorkMem uow, E_LoginMem userMem, E_FolderMem folderMem)
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
