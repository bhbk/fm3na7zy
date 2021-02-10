using Bhbk.Lib.Aurora.Data_EF6.Models;
using Bhbk.Lib.Aurora.Data_EF6.UnitOfWork;
using Bhbk.Lib.QueryExpression.Extensions;
using Bhbk.Lib.QueryExpression.Factories;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Bhbk.Daemon.Aurora.SFTP.Factories
{
    internal class DatabasePathFactory
    {
        internal static Folder_EF CheckFolder(IUnitOfWork uow, Login_EF user)
        {
            var callPath = $"{MethodBase.GetCurrentMethod().DeclaringType.Name}.{MethodBase.GetCurrentMethod().Name}";

            var folder = uow.Folders.Get(QueryExpressionFactory.GetQueryExpression<Folder_EF>()
                .Where(x => x.UserId == user.UserId && x.ParentId == null).ToLambda())
                .SingleOrDefault();

            if (folder == null)
            {
                folder = uow.Folders.Create(
                    new Folder_EF
                    {
                        UserId = user.UserId,
                        ParentId = null,
                        VirtualName = string.Empty,
                        CreatedUtc = DateTime.UtcNow,
                        IsReadOnly = true,
                    });
                uow.Commit();

                Log.Information($"'{callPath}' '{user.UserName}' folder:'/' at:database");
            }

            return folder;
        }

        internal static File_EF PathToFile(IUnitOfWork uow, Login_EF user, string path)
        {
            if (path.FirstOrDefault() == '/')
                path = path.Substring(1);

            var pathBits = path.Split("/");
            var filePath = path.Split("/").Last();
            var folderPath = string.Empty;

            for (int i = 0; i <= pathBits.Count() - 2; i++)
                folderPath += "/" + pathBits.ElementAt(i);

            var folder = PathToFolder(uow, user, folderPath);

            var file = uow.Files.Get(QueryExpressionFactory.GetQueryExpression<File_EF>()
                .Where(x => x.UserId == user.UserId && x.FolderId == folder.Id && x.VirtualName == filePath).ToLambda())
                .SingleOrDefault();

            return file;
        }

        internal static Folder_EF PathToFolder(IUnitOfWork uow, Login_EF user, string path)
        {
            if (path.FirstOrDefault() == '/')
                path = path.Substring(1);

            var folder = uow.Folders.Get(QueryExpressionFactory.GetQueryExpression<Folder_EF>()
                .Where(x => x.UserId == user.UserId && x.ParentId == null).ToLambda())
                .SingleOrDefault();

            if (string.IsNullOrWhiteSpace(path))
                return folder;

            foreach (var entry in path.Split("/"))
            {
                folder = uow.Folders.Get(QueryExpressionFactory.GetQueryExpression<Folder_EF>()
                    .Where(x => x.UserId == user.UserId && x.ParentId == folder.Id && x.VirtualName == entry).ToLambda())
                    .SingleOrDefault();
            };

            return folder;
        }

        internal static string FileToPath(IUnitOfWork uow, Login_EF user, File_EF file)
        {
            var path = string.Empty;
            var paths = new List<string> { };

            var folder = uow.Folders.Get(QueryExpressionFactory.GetQueryExpression<Folder_EF>()
                .Where(x => x.UserId == user.UserId && x.Id == file.FolderId).ToLambda())
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

        internal static string FolderToPath(IUnitOfWork uow, Login_EF user, Folder_EF folder)
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
