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

        public virtual DbSet<uvw_Alert> uvw_Alerts { get; set; }
        public virtual DbSet<uvw_Ambassador> uvw_Ambassadors { get; set; }
        public virtual DbSet<uvw_File> uvw_Files { get; set; }
        public virtual DbSet<uvw_Folder> uvw_Folders { get; set; }
        public virtual DbSet<uvw_Login> uvw_Logins { get; set; }
        public virtual DbSet<uvw_Mount> uvw_Mounts { get; set; }
        public virtual DbSet<uvw_Network> uvw_Networks { get; set; }
        public virtual DbSet<uvw_PrivateKey> uvw_PrivateKeys { get; set; }
        public virtual DbSet<uvw_PublicKey> uvw_PublicKeys { get; set; }
        public virtual DbSet<uvw_Session> uvw_Sessions { get; set; }
        public virtual DbSet<uvw_Setting> uvw_Settings { get; set; }
        public virtual DbSet<uvw_LoginUsage> uvw_Usages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("Relational:Collation", "SQL_Latin1_General_CP1_CI_AS");

            modelBuilder.Entity<uvw_Alert>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("uvw_Alert", "svc");

                entity.Property(e => e.ToDisplayName)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.Property(e => e.ToEmailAddress).HasMaxLength(320);

                entity.Property(e => e.ToPhoneNumber).HasMaxLength(15);
            });

            modelBuilder.Entity<uvw_Ambassador>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("uvw_Ambassador", "svc");

                entity.Property(e => e.EncryptedPass)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.Property(e => e.UserName)
                    .IsRequired()
                    .HasMaxLength(128);
            });

            modelBuilder.Entity<uvw_File>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("uvw_File", "svc");

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

            modelBuilder.Entity<uvw_Folder>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("uvw_Folder", "svc");

                entity.Property(e => e.VirtualName).IsRequired();
            });

            modelBuilder.Entity<uvw_Login>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("uvw_Login", "svc");

                entity.Property(e => e.Debugger).HasMaxLength(16);

                entity.Property(e => e.EncryptedPass).HasMaxLength(1024);

                entity.Property(e => e.FileSystemChrootPath).HasMaxLength(64);

                entity.Property(e => e.FileSystemType)
                    .IsRequired()
                    .HasMaxLength(16);

                entity.Property(e => e.UserLoginType)
                    .IsRequired()
                    .HasMaxLength(16);

                entity.Property(e => e.UserName)
                    .IsRequired()
                    .HasMaxLength(128);
            });

            modelBuilder.Entity<uvw_Mount>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("uvw_Mount", "svc");

                entity.Property(e => e.AuthType)
                    .IsRequired()
                    .HasMaxLength(16);

                entity.Property(e => e.ServerAddress)
                    .IsRequired()
                    .HasMaxLength(256);

                entity.Property(e => e.ServerShare)
                    .IsRequired()
                    .HasMaxLength(256);
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

                entity.Property(e => e.EncryptedPass)
                    .IsRequired()
                    .HasMaxLength(1024);

                entity.Property(e => e.KeyAlgo)
                    .IsRequired()
                    .HasMaxLength(16);

                entity.Property(e => e.KeyFormat)
                    .IsRequired()
                    .HasMaxLength(16);

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

            modelBuilder.Entity<uvw_Session>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("uvw_Session", "svc");

                entity.Property(e => e.CallPath)
                    .IsRequired()
                    .HasMaxLength(256);

                entity.Property(e => e.LocalEndPoint)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.Property(e => e.LocalSoftwareIdentifier).HasMaxLength(128);

                entity.Property(e => e.RemoteEndPoint)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.Property(e => e.RemoteSoftwareIdentifier).HasMaxLength(128);

                entity.Property(e => e.UserName).HasMaxLength(128);
            });

            modelBuilder.Entity<uvw_Setting>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("uvw_Setting", "svc");

                entity.Property(e => e.ConfigKey)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.Property(e => e.ConfigValue)
                    .IsRequired()
                    .HasMaxLength(256);
            });

            modelBuilder.Entity<uvw_LoginUsage>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("uvw_Usage", "svc");

                entity.Property(e => e.UserName).HasMaxLength(128);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
