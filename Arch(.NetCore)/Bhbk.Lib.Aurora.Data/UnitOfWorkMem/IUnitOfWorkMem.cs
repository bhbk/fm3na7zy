using Bhbk.Lib.Aurora.Data.ModelsMem;
using Bhbk.Lib.DataAccess.EFCore.Repositories;
using Bhbk.Lib.DataAccess.EFCore.UnitOfWorks;

namespace Bhbk.Lib.Aurora.Data.UnitOfWorkMem
{
    public interface IUnitOfWorkMem : IGenericUnitOfWork
    {
        IGenericRepository<E_LoginMem> UserLogins { get; }
        IGenericRepository<E_FileMem> UserFiles { get; }
        IGenericRepository<E_FolderMem> UserFolders { get; }
    }
}
