﻿using Bhbk.Lib.Aurora.Data_EF6.Models;
using Bhbk.Lib.Aurora.Data_EF6.UnitOfWorks;
using Bhbk.Lib.QueryExpression.Extensions;
using Bhbk.Lib.QueryExpression.Factories;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Bhbk.Lib.Aurora.Domain.Providers
{
    public class DatabaseProvider
    {
        public static Folder_EF CheckFolder(IUnitOfWork uow, FileSystemLogin_EF fileSystemLogin)
        {
            var callPath = $"{MethodBase.GetCurrentMethod().DeclaringType.Name}.{MethodBase.GetCurrentMethod().Name}";

            var folder = uow.Folders.Get(QueryExpressionFactory.GetQueryExpression<Folder_EF>()
                .Where(x => x.FileSystemId == fileSystemLogin.FileSystem.Id && x.ParentId == null).ToLambda())
                .SingleOrDefault();

            if (folder == null)
            {
                folder = uow.Folders.Create(
                    new Folder_EF
                    {
                        FileSystemId = fileSystemLogin.FileSystem.Id,
                        ParentId = null,
                        VirtualName = string.Empty,
                        CreatorId = fileSystemLogin.Login.UserId,
                        CreatedUtc = DateTime.UtcNow,
                        IsReadOnly = true,
                    });
                uow.Commit();

                Log.Information($"'{callPath}' '{fileSystemLogin.Login.UserName}' folder:'/' at:database");
            }

            return folder;
        }

        public static File_EF PathToFile(IUnitOfWork uow, FileSystemLogin_EF fileSystemLogin, string path)
        {
            if (path.FirstOrDefault() == '/')
                path = path.Substring(1);

            var pathBits = path.Split("/");
            var filePath = path.Split("/").Last();
            var folderPath = string.Empty;

            for (int i = 0; i <= pathBits.Count() - 2; i++)
                folderPath += "/" + pathBits.ElementAt(i);

            var folder = PathToFolder(uow, fileSystemLogin, folderPath);

            var file = uow.Files.Get(QueryExpressionFactory.GetQueryExpression<File_EF>()
                .Where(x => x.FileSystemId == fileSystemLogin.FileSystem.Id && x.FolderId == folder.Id && x.VirtualName == filePath).ToLambda())
                .SingleOrDefault();

            return file;
        }

        public static Folder_EF PathToFolder(IUnitOfWork uow, FileSystemLogin_EF fileSystemLogin, string path)
        {
            if (path.FirstOrDefault() == '/')
                path = path.Substring(1);

            var folder = uow.Folders.Get(QueryExpressionFactory.GetQueryExpression<Folder_EF>()
                .Where(x => x.FileSystemId == fileSystemLogin.FileSystem.Id && x.ParentId == null).ToLambda())
                .SingleOrDefault();

            if (string.IsNullOrWhiteSpace(path))
                return folder;

            foreach (var entry in path.Split("/"))
            {
                folder = uow.Folders.Get(QueryExpressionFactory.GetQueryExpression<Folder_EF>()
                    .Where(x => x.FileSystemId == fileSystemLogin.FileSystem.Id && x.ParentId == folder.Id && x.VirtualName == entry).ToLambda())
                    .SingleOrDefault();
            };

            return folder;
        }

        public static string FileToPath(IUnitOfWork uow, FileSystemLogin_EF fileSystemLogin, File_EF file)
        {
            var path = string.Empty;
            var paths = new List<string> { };

            var folder = uow.Folders.Get(QueryExpressionFactory.GetQueryExpression<Folder_EF>()
                .Where(x => x.FileSystemId == fileSystemLogin.FileSystem.Id && x.Id == file.FolderId).ToLambda())
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

        public static string FolderToPath(IUnitOfWork uow, FileSystemLogin_EF fileSystemLogin, Folder_EF folder)
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
