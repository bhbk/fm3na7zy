using AutoMapper;
using System;
using Bhbk.Lib.Aurora.Data.ModelsMem;
using Bhbk.Lib.Aurora.Data.UnitOfWorksMem;
using Bhbk.Lib.Aurora.Data_EF6.Models;
using Bhbk.Lib.Aurora.Domain.Helpers;
using Bhbk.Lib.Aurora.Domain.Providers;
using Bhbk.Lib.Common.Primitives;
using Bhbk.Lib.QueryExpression.Extensions;
using Bhbk.Lib.QueryExpression.Factories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Rebex.IO.FileSystem;
using Serilog;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using Hashing = Bhbk.Lib.Cryptography.Hashing;

namespace Bhbk.Lib.Aurora.Domain.Profiles
{
    public class AutoMapperProfile_EF6 : Profile
    {
        public AutoMapperProfile_EF6()
        {
            CreateMap<File_EF, FileMem>()
                .ReverseMap();

            CreateMap<FileSystem_EF, FileSystemMem>()
                .ReverseMap();

            CreateMap<FileSystemLogin_EF, FileSystemLoginMem>()
                .ReverseMap();

            CreateMap<FileSystemUsage_EF, FileSystemUsageMem>()
                .ReverseMap();

            CreateMap<Folder_EF, FolderMem>()
                .ReverseMap();

            CreateMap<Login_EF, LoginMem>()
                .ReverseMap();
        }
    }
}
