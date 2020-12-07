using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Core.Objects;
using System.Linq;

namespace Bhbk.Lib.Aurora.Data_EF6.ModelsMem
{
    public partial class AuroraEntitiesMem : DbContext
    {
        public AuroraEntitiesMem()
            : base("AuroraEntitiesMem")
        {

        }

        public virtual DbSet<UserLoginMem> Users { get; set; }
        public virtual DbSet<UserFileMem> UserFiles { get; set; }
        public virtual DbSet<UserFolderMem> UserFolders { get; set; }
    }
}
