using Bhbk.Lib.Aurora.Data_EF6.ModelsMem;
using Bhbk.Lib.DataAccess.EF.Repositories;
using Bhbk.Lib.DataAccess.EF.UnitOfWorks;

namespace Bhbk.Lib.Aurora.Data_EF6.UnitOfWorkMem
{
    public interface IUnitOfWorkMem : IGenericUnitOfWork
    {
        IGenericRepository<UserLoginMem> UserLogins { get; }
        IGenericRepository<UserFileMem> UserFiles { get; }
        IGenericRepository<UserFolderMem> UserFolders { get; }
    }
}
