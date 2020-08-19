using Bhbk.Lib.Aurora.Data.Models_DIRECT;
using Bhbk.Lib.DataAccess.EFCore.Repositories;

namespace Bhbk.Lib.Aurora.Data.Repositories_DIRECT
{
    public class PublicKeyRepository : GenericRepository<tbl_PublicKey>
    {
        public PublicKeyRepository(AuroraEntities context)
            : base(context) { }
    }
}
