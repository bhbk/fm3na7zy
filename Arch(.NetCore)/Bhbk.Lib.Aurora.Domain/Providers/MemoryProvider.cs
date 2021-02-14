using Bhbk.Lib.Aurora.Data.ModelsMem;
using Bhbk.Lib.Aurora.Data.UnitOfWorksMem;
using Bhbk.Lib.Aurora.Data_EF6.Models;
using Bhbk.Lib.Aurora.Primitives.Enums;
using Bhbk.Lib.QueryExpression.Extensions;
using Bhbk.Lib.QueryExpression.Factories;
using Serilog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Bhbk.Lib.Aurora.Domain.Providers
{
    public class MemoryProvider
    {
        public static FileSystemLoginMem CheckFileSystemLogin(IUnitOfWorkMem uow, FileSystemLogin_EF fileSystemLogin)
        {
            var callPath = $"{MethodBase.GetCurrentMethod().DeclaringType.Name}.{MethodBase.GetCurrentMethod().Name}";

            var fileSystemMem = uow.FileSystems.Get(QueryExpressionFactory.GetQueryExpression<FileSystemMem>()
                .Where(x => x.Id == fileSystemLogin.FileSystemId).ToLambda())
                .SingleOrDefault();

            if (fileSystemMem == null)
            {
                fileSystemMem = uow.FileSystems.Create(
                    new FileSystemMem
                    {
                        Id = fileSystemLogin.FileSystemId,
                        FileSystemTypeId = (int)FileSystemType_E.Memory,
                    });
                uow.Commit();

                fileSystemMem.Usage = uow.FileSystemUsages.Create(
                    new FileSystemUsageMem
                    {
                        FileSystemId = fileSystemLogin.FileSystemId,
                        QuotaInBytes = fileSystemLogin.FileSystem.Usage.QuotaInBytes,
                        QuotaUsedInBytes = 0,
                    });
                uow.Commit();

                Log.Information($"'{callPath}' '{fileSystemLogin.FileSystem.Name}' exists at:memory");
            }

            var userMem = uow.Logins.Get(QueryExpressionFactory.GetQueryExpression<LoginMem>()
                .Where(x => x.UserId == fileSystemLogin.UserId).ToLambda())
                .SingleOrDefault();

            if (userMem == null)
            {
                userMem = uow.Logins.Create(
                    new LoginMem
                    {
                        UserId = fileSystemLogin.UserId,
                        UserName = fileSystemLogin.Login.UserName,
                    });
                uow.Commit();

                Log.Information($"'{callPath}' '{fileSystemLogin.Login.UserName}' exists at:memory");
            }

            var fileSystemLoginMem = uow.FileSystemLogins.Get(QueryExpressionFactory.GetQueryExpression<FileSystemLoginMem>()
                .Where(x => x.FileSystemId == fileSystemLogin.FileSystemId && x.UserId == fileSystemLogin.UserId).ToLambda())
                .SingleOrDefault();

            if (fileSystemLoginMem == null)
            {
                fileSystemLoginMem = uow.FileSystemLogins.Create(
                    new FileSystemLoginMem
                    {
                        FileSystemId = fileSystemLogin.FileSystemId,
                        UserId = fileSystemLogin.UserId,
                    });
                uow.Commit();
            }

            return uow.FileSystemLogins.Get(QueryExpressionFactory.GetQueryExpression<FileSystemLoginMem>()
                .Where(x => x.FileSystemId == fileSystemLogin.FileSystemId && x.UserId == fileSystemLogin.UserId).ToLambda(),
                    new List<Expression<Func<FileSystemLoginMem, object>>>()
                    {
                        x => x.FileSystem,
                        x => x.FileSystem.Usage,
                        x => x.User,
                    })
                .Single();
        }

        public static FolderMem CheckFolder(IUnitOfWorkMem uow, FileSystemLoginMem fileSystemLogin)
        {
            var callPath = $"{MethodBase.GetCurrentMethod().DeclaringType.Name}.{MethodBase.GetCurrentMethod().Name}";

            var folderMem = uow.Folders.Get(QueryExpressionFactory.GetQueryExpression<FolderMem>()
                .Where(x => x.FileSystemId == fileSystemLogin.FileSystem.Id && x.ParentId == null).ToLambda())
                .SingleOrDefault();

            if (folderMem == null)
            {
                folderMem = uow.Folders.Create(
                    new FolderMem
                    {
                        Id = Guid.NewGuid(),
                        FileSystemId = fileSystemLogin.FileSystemId,
                        ParentId = null,
                        VirtualName = string.Empty,
                        CreatorId = fileSystemLogin.UserId,
                        CreatedUtc = DateTime.UtcNow,
                        IsReadOnly = true,
                    });
                uow.Commit();

                Log.Information($"'{callPath}' '{fileSystemLogin.User.UserName}' folder:'/' at:memory");
            }

            return folderMem;
        }

        public static FileMem PathToFile(IUnitOfWorkMem uow, FileSystemLoginMem fileSystemLogin, string path)
        {
            if (path.FirstOrDefault() == '/')
                path = path.Substring(1);

            var pathBits = path.Split("/");
            var filePath = path.Split("/").Last();
            var folderPath = string.Empty;

            for (int i = 0; i <= pathBits.Count() - 2; i++)
                folderPath += "/" + pathBits.ElementAt(i);

            var folder = PathToFolder(uow, fileSystemLogin, folderPath);

            var file = uow.Files.Get(QueryExpressionFactory.GetQueryExpression<FileMem>()
                .Where(x => x.FileSystemId == fileSystemLogin.FileSystem.Id && x.FolderId == folder.Id && x.VirtualName == filePath).ToLambda())
                .SingleOrDefault();

            return file;
        }

        public static FolderMem PathToFolder(IUnitOfWorkMem uow, FileSystemLoginMem fileSystemLogin, string path)
        {
            if (path.FirstOrDefault() == '/')
                path = path.Substring(1);

            var folder = uow.Folders.Get(QueryExpressionFactory.GetQueryExpression<FolderMem>()
                .Where(x => x.FileSystemId == fileSystemLogin.FileSystem.Id && x.ParentId == null).ToLambda())
                .SingleOrDefault();

            if (string.IsNullOrWhiteSpace(path))
                return folder;

            foreach (var entry in path.Split("/"))
            {
                folder = uow.Folders.Get(QueryExpressionFactory.GetQueryExpression<FolderMem>()
                    .Where(x => x.FileSystemId == fileSystemLogin.FileSystem.Id && x.ParentId == folder.Id && x.VirtualName == entry).ToLambda())
                    .SingleOrDefault();
            };

            return folder;
        }

        public static string FileToPath(IUnitOfWorkMem uow, FileSystemLoginMem fileSystemLogin, FileMem fileMem)
        {
            var path = string.Empty;
            var paths = new List<string> { };

            var folder = uow.Folders.Get(QueryExpressionFactory.GetQueryExpression<FolderMem>()
                .Where(x => x.FileSystemId == fileSystemLogin.FileSystem.Id && x.Id == fileMem.FolderId).ToLambda())
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

        public static string FolderToPath(IUnitOfWorkMem uow, FileSystemLoginMem fileSystemLogin, FolderMem folderMem)
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
