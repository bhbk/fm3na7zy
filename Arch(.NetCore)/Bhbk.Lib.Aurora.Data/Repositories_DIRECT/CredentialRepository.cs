using Bhbk.Lib.Aurora.Data.Models_DIRECT;
using Bhbk.Lib.DataAccess.EFCore.Repositories;

namespace Bhbk.Lib.Aurora.Data.Repositories_DIRECT
{
    public class CredentialRepository : GenericRepository<tbl_Credential>
    {
        public CredentialRepository(AuroraEntities context)
            : base(context) { }
    }
}
