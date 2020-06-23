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

        public virtual DbSet<uvw_SysCredentials> uvw_SysCredentials { get; set; }
        public virtual DbSet<uvw_SysPrivateKeys> uvw_SysPrivateKeys { get; set; }
        public virtual DbSet<uvw_SysSettings> uvw_SysSettings { get; set; }
        public virtual DbSet<uvw_UserFiles> uvw_UserFiles { get; set; }
        public virtual DbSet<uvw_UserFolders> uvw_UserFolders { get; set; }
        public virtual DbSet<uvw_UserMounts> uvw_UserMounts { get; set; }
        public virtual DbSet<uvw_UserPasswords> uvw_UserPasswords { get; set; }
        public virtual DbSet<uvw_UserPrivateKeys> uvw_UserPrivateKeys { get; set; }
        public virtual DbSet<uvw_UserPublicKeys> uvw_UserPublicKeys { get; set; }
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
            modelBuilder.Entity<uvw_SysCredentials>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("uvw_SysCredentials", "svc");

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

            modelBuilder.Entity<uvw_SysPrivateKeys>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("uvw_SysPrivateKeys", "svc");

                entity.Property(e => e.KeyValueAlgo)
                    .IsRequired()
                    .HasMaxLength(16);

                entity.Property(e => e.KeyValueBase64).IsRequired();

                entity.Property(e => e.KeyValueFormat)
                    .IsRequired()
                    .HasMaxLength(16);

                entity.Property(e => e.KeyValuePass)
                    .IsRequired()
                    .HasMaxLength(1024);
            });

            modelBuilder.Entity<uvw_SysSettings>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("uvw_SysSettings", "svc");

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

                entity.Property(e => e.ServerName)
                    .IsRequired()
                    .HasMaxLength(256)
                    .IsUnicode(false);

                entity.Property(e => e.ServerPath)
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

                entity.Property(e => e.PasswordHashPBKDF2).HasMaxLength(2048);

                entity.Property(e => e.PasswordHashSHA256).HasMaxLength(2048);

                entity.Property(e => e.SecurityStamp)
                    .IsRequired()
                    .HasMaxLength(1024);
            });

            modelBuilder.Entity<uvw_UserPrivateKeys>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("uvw_UserPrivateKeys", "svc");

                entity.Property(e => e.KeyValueAlgo)
                    .IsRequired()
                    .HasMaxLength(16);

                entity.Property(e => e.KeyValueBase64).IsRequired();

                entity.Property(e => e.KeyValuePass)
                    .IsRequired()
                    .HasMaxLength(1024);
            });

            modelBuilder.Entity<uvw_UserPublicKeys>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("uvw_UserPublicKeys", "svc");

                entity.Property(e => e.Hostname)
                    .IsRequired()
                    .HasMaxLength(1024);

                entity.Property(e => e.KeySig)
                    .IsRequired()
                    .HasMaxLength(512);

                entity.Property(e => e.KeySigAlgo)
                    .IsRequired()
                    .HasMaxLength(16);

                entity.Property(e => e.KeyValueAlgo)
                    .IsRequired()
                    .HasMaxLength(16);

                entity.Property(e => e.KeyValueBase64).IsRequired();
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
