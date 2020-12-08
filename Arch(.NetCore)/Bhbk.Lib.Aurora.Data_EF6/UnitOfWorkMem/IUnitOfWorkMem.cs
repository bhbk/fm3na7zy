using Bhbk.Lib.Aurora.Data_EF6.ModelsMem;
using Bhbk.Lib.DataAccess.EF.Repositories;
using Bhbk.Lib.DataAccess.EF.UnitOfWorks;

namespace Bhbk.Lib.Aurora.Data_EF6.UnitOfWorkMem
{
    public interface IUnitOfWorkMem : IGenericUnitOfWork
    {
        IGenericRepository<E_LoginMem> UserLogins { get; }
        IGenericRepository<E_FileMem> UserFiles { get; }
        IGenericRepository<E_FolderMem> UserFolders { get; }
    }
}
