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

        public virtual DbSet<E_LoginMem> Users { get; set; }
        public virtual DbSet<E_FileMem> UserFiles { get; set; }
        public virtual DbSet<E_FolderMem> UserFolders { get; set; }
    }
}
