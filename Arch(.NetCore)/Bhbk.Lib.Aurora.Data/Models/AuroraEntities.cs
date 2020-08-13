using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Bhbk.Lib.Aurora.Data.Models
{
    public partial class AuroraEntities : DbContext
    {
        public AuroraEntities()
        {
        }

        public AuroraEntities(DbContextOptions<AuroraEntities> options)
            : base(options)
        {
        }

        public virtual DbSet<uvw_Ambassadors> uvw_Ambassadors { get; set; }
        public virtual DbSet<uvw_PrivateKeys> uvw_PrivateKeys { get; set; }
        public virtual DbSet<uvw_PublicKeys> uvw_PublicKeys { get; set; }
        public virtual DbSet<uvw_Realms> uvw_Realms { get; set; }
        public virtual DbSet<uvw_Settings> uvw_Settings { get; set; }
        public virtual DbSet<uvw_UserFiles> uvw_UserFiles { get; set; }
        public virtual DbSet<uvw_UserFolders> uvw_UserFolders { get; set; }
        public virtual DbSet<uvw_UserMounts> uvw_UserMounts { get; set; }
        public virtual DbSet<uvw_UserPasswords> uvw_UserPasswords { get; set; }
        public virtual DbSet<uvw_UserRealms> uvw_UserRealms { get; set; }
        public virtual DbSet<uvw_Users> uvw_Users { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. See http://go.microsoft.com/fwlink/?LinkId=723263 for guidance on storing connection strings.
                optionsBuilder.UseSqlServer("Data Source=bits.test.ochap.local; Initial Catalog=BhbkAurora; User ID=Sql.BhbkAurora; Password=Pa$$word01!");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<uvw_Ambassadors>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("uvw_Ambassadors", "svc");

                entity.Property(e => e.Domain)
                    .HasMaxLength(128)
                    .IsUnicode(false);

                entity.Property(e => e.Password)
                    .IsRequired()
                    .HasMaxLength(128)
                    .IsUnicode(false);

                entity.Property(e => e.UserName)
                    .IsRequired()
                    .HasMaxLength(128)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<uvw_PrivateKeys>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("uvw_PrivateKeys", "svc");

                entity.Property(e => e.KeyAlgo)
                    .IsRequired()
                    .HasMaxLength(16);

                entity.Property(e => e.KeyFormat)
                    .IsRequired()
                    .HasMaxLength(16);

                entity.Property(e => e.KeyPass)
                    .IsRequired()
                    .HasMaxLength(1024);

                entity.Property(e => e.KeyValue).IsRequired();
            });

            modelBuilder.Entity<uvw_PublicKeys>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("uvw_PublicKeys", "svc");

                entity.Property(e => e.Hostname).HasMaxLength(1024);

                entity.Property(e => e.KeyAlgo)
                    .IsRequired()
                    .HasMaxLength(16);

                entity.Property(e => e.KeyFormat)
                    .IsRequired()
                    .HasMaxLength(16);

                entity.Property(e => e.KeyValue).IsRequired();

                entity.Property(e => e.SigAlgo)
                    .IsRequired()
                    .HasMaxLength(16);

                entity.Property(e => e.SigValue)
                    .IsRequired()
                    .HasMaxLength(512);
            });

            modelBuilder.Entity<uvw_Realms>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("uvw_Realms", "svc");

                entity.Property(e => e.Name)
                    .HasMaxLength(10)
                    .IsFixedLength();
            });

            modelBuilder.Entity<uvw_Settings>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("uvw_Settings", "svc");

                entity.Property(e => e.ConfigKey)
                    .IsRequired()
                    .HasMaxLength(128)
                    .IsUnicode(false);

                entity.Property(e => e.ConfigValue)
                    .IsRequired()
                    .HasMaxLength(256)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<uvw_UserFiles>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("uvw_UserFiles", "svc");

                entity.Property(e => e.HashSHA256).HasMaxLength(64);

                entity.Property(e => e.RealFileName).IsRequired();

                entity.Property(e => e.RealPath).IsRequired();

                entity.Property(e => e.VirtualName).IsRequired();
            });

            modelBuilder.Entity<uvw_UserFolders>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("uvw_UserFolders", "svc");

                entity.Property(e => e.VirtualName)
                    .IsRequired()
                    .IsUnicode(false);
            });

            modelBuilder.Entity<uvw_UserMounts>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("uvw_UserMounts", "svc");

                entity.Property(e => e.AuthType)
                    .IsRequired()
                    .HasMaxLength(16)
                    .IsUnicode(false);

                entity.Property(e => e.ServerAddress)
                    .IsRequired()
                    .HasMaxLength(256)
                    .IsUnicode(false);

                entity.Property(e => e.ServerShare)
                    .IsRequired()
                    .HasMaxLength(256)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<uvw_UserPasswords>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("uvw_UserPasswords", "svc");

                entity.Property(e => e.ConcurrencyStamp)
                    .IsRequired()
                    .HasMaxLength(1024);

                entity.Property(e => e.HashPBKDF2).HasMaxLength(2048);

                entity.Property(e => e.HashSHA256).HasMaxLength(2048);

                entity.Property(e => e.SecurityStamp)
                    .IsRequired()
                    .HasMaxLength(1024);
            });

            modelBuilder.Entity<uvw_UserRealms>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("uvw_UserRealms", "svc");
            });

            modelBuilder.Entity<uvw_Users>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("uvw_Users", "svc");

                entity.Property(e => e.DebugLevel)
                    .HasMaxLength(16)
                    .IsUnicode(false);

                entity.Property(e => e.FileSystemType)
                    .IsRequired()
                    .HasMaxLength(16)
                    .IsUnicode(false);

                entity.Property(e => e.UserName)
                    .IsRequired()
                    .HasMaxLength(128)
                    .IsUnicode(false);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
