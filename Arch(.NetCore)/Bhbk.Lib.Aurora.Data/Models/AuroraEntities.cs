using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

#nullable disable

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

        public virtual DbSet<uvw_Credential> uvw_Credentials { get; set; }
        public virtual DbSet<uvw_Network> uvw_Networks { get; set; }
        public virtual DbSet<uvw_PrivateKey> uvw_PrivateKeys { get; set; }
        public virtual DbSet<uvw_PublicKey> uvw_PublicKeys { get; set; }
        public virtual DbSet<uvw_Setting> uvw_Settings { get; set; }
        public virtual DbSet<uvw_User> uvw_Users { get; set; }
        public virtual DbSet<uvw_UserAlert> uvw_UserAlerts { get; set; }
        public virtual DbSet<uvw_UserFile> uvw_UserFiles { get; set; }
        public virtual DbSet<uvw_UserFolder> uvw_UserFolders { get; set; }
        public virtual DbSet<uvw_UserMount> uvw_UserMounts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("Relational:Collation", "SQL_Latin1_General_CP1_CI_AS");

            modelBuilder.Entity<uvw_Credential>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("uvw_Credential", "svc");

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

            modelBuilder.Entity<uvw_Network>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("uvw_Network", "svc");

                entity.Property(e => e.Action)
                    .IsRequired()
                    .HasMaxLength(8);

                entity.Property(e => e.Address)
                    .IsRequired()
                    .HasMaxLength(128);
            });

            modelBuilder.Entity<uvw_PrivateKey>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("uvw_PrivateKey", "svc");

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

            modelBuilder.Entity<uvw_PublicKey>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("uvw_PublicKey", "svc");

                entity.Property(e => e.Comment).HasMaxLength(1024);

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

            modelBuilder.Entity<uvw_Setting>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("uvw_Setting", "svc");

                entity.Property(e => e.ConfigKey)
                    .IsRequired()
                    .HasMaxLength(128)
                    .IsUnicode(false);

                entity.Property(e => e.ConfigValue)
                    .IsRequired()
                    .HasMaxLength(256)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<uvw_User>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("uvw_User", "svc");

                entity.Property(e => e.Debugger)
                    .HasMaxLength(16)
                    .IsUnicode(false);

                entity.Property(e => e.FileSystemType)
                    .IsRequired()
                    .HasMaxLength(16)
                    .IsUnicode(false);

                entity.Property(e => e.IdentityAlias)
                    .IsRequired()
                    .HasMaxLength(128)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<uvw_UserAlert>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("uvw_UserAlert", "svc");

                entity.Property(e => e.ToEmailAddress)
                    .HasMaxLength(320)
                    .IsUnicode(false);

                entity.Property(e => e.ToFirstName)
                    .IsRequired()
                    .HasMaxLength(128)
                    .IsUnicode(false);

                entity.Property(e => e.ToLastName)
                    .IsRequired()
                    .HasMaxLength(128)
                    .IsUnicode(false);

                entity.Property(e => e.ToPhoneNumber)
                    .HasMaxLength(15)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<uvw_UserFile>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("uvw_UserFile", "svc");

                entity.Property(e => e.HashSHA256)
                    .IsRequired()
                    .HasMaxLength(64);

                entity.Property(e => e.RealFileName)
                    .IsRequired()
                    .HasMaxLength(260);

                entity.Property(e => e.RealPath).IsRequired();

                entity.Property(e => e.VirtualName)
                    .IsRequired()
                    .HasMaxLength(260);
            });

            modelBuilder.Entity<uvw_UserFolder>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("uvw_UserFolder", "svc");

                entity.Property(e => e.VirtualName)
                    .IsRequired()
                    .IsUnicode(false);
            });

            modelBuilder.Entity<uvw_UserMount>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("uvw_UserMount", "svc");

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

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
