using Bhbk.Lib.Aurora.Data.ModelsMem;
using Bhbk.Lib.DataAccess.EFCore.Repositories;
using Bhbk.Lib.DataAccess.EFCore.UnitOfWorks;

namespace Bhbk.Lib.Aurora.Data.UnitOfWorksMem
{
    public interface IUnitOfWorkMem : IGenericUnitOfWork
    {
        IGenericRepository<FileMem> Files { get; }
        IGenericRepository<FileSystemMem> FileSystems { get; }
        IGenericRepository<FileSystemLoginMem> FileSystemLogins { get; }
        IGenericRepository<FileSystemUsageMem> FileSystemUsages { get; }
        IGenericRepository<FolderMem> Folders { get; }
        IGenericRepository<LoginMem> Logins { get; }
    }
}
