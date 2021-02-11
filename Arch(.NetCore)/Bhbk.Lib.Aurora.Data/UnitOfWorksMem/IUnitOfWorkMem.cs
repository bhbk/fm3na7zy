using Bhbk.Lib.Aurora.Data.ModelsMem;
using Bhbk.Lib.DataAccess.EFCore.Repositories;
using Bhbk.Lib.DataAccess.EFCore.UnitOfWorks;

namespace Bhbk.Lib.Aurora.Data.UnitOfWorksMem
{
    public interface IUnitOfWorkMem : IGenericUnitOfWork
    {
        IGenericRepository<LoginMem_EF> Logins { get; }
        IGenericRepository<FileMem_EF> Files { get; }
        IGenericRepository<FolderMem_EF> Folders { get; }
    }
}
