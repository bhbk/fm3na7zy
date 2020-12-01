using Bhbk.Lib.Aurora.Data.ModelsMem;
using Bhbk.Lib.DataAccess.EFCore.Repositories;
using Bhbk.Lib.DataAccess.EFCore.UnitOfWorks;

namespace Bhbk.Lib.Aurora.Data.UnitOfWorkMem
{
    public interface IUnitOfWorkMem : IGenericUnitOfWork
    {
        IGenericRepository<UserMem> Users { get; }
        IGenericRepository<UserFileMem> UserFiles { get; }
        IGenericRepository<UserFolderMem> UserFolders { get; }
    }
}
