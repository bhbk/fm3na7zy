using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

#nullable disable

namespace Bhbk.Lib.Aurora.Data.Models_DIRECT
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

        public virtual DbSet<tbl_Activity> tbl_Activities { get; set; }
        public virtual DbSet<tbl_Credential> tbl_Credentials { get; set; }
        public virtual DbSet<tbl_Network> tbl_Networks { get; set; }
        public virtual DbSet<tbl_PrivateKey> tbl_PrivateKeys { get; set; }
        public virtual DbSet<tbl_PublicKey> tbl_PublicKeys { get; set; }
        public virtual DbSet<tbl_Setting> tbl_Settings { get; set; }
        public virtual DbSet<tbl_User> tbl_Users { get; set; }
        public virtual DbSet<tbl_UserFile> tbl_UserFiles { get; set; }
        public virtual DbSet<tbl_UserFolder> tbl_UserFolders { get; set; }
        public virtual DbSet<tbl_UserMount> tbl_UserMounts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("Relational:Collation", "SQL_Latin1_General_CP1_CI_AS");

            modelBuilder.Entity<tbl_Activity>(entity =>
            {
                entity.ToTable("tbl_Activity");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.ActivityType)
                    .IsRequired()
                    .HasMaxLength(64);

                entity.Property(e => e.TableName).HasMaxLength(256);

                entity.HasOne(d => d.Identity)
                    .WithMany(p => p.tbl_Activities)
                    .HasForeignKey(d => d.IdentityId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_tbl_Activity_UserID");
            });

            modelBuilder.Entity<tbl_Credential>(entity =>
            {
                entity.ToTable("tbl_Credential");

                entity.HasIndex(e => e.Id, "IX_tbl_SysCredentials")
                    .IsUnique();

                entity.Property(e => e.Id).ValueGeneratedNever();

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

            modelBuilder.Entity<tbl_Network>(entity =>
            {
                entity.ToTable("tbl_Network");

                entity.HasIndex(e => e.Id, "IX_tbl_Networks")
                    .IsUnique();

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Action)
                    .IsRequired()
                    .HasMaxLength(8);

                entity.Property(e => e.Address)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.HasOne(d => d.Identity)
                    .WithMany(p => p.tbl_Networks)
                    .HasForeignKey(d => d.IdentityId)
                    .HasConstraintName("FK_tbl_Network_IdentityID");
            });

            modelBuilder.Entity<tbl_PrivateKey>(entity =>
            {
                entity.ToTable("tbl_PrivateKey");

                entity.HasIndex(e => e.Id, "IX_tbl_UserPrivateKeys")
                    .IsUnique();

                entity.Property(e => e.Id).ValueGeneratedNever();

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

                entity.HasOne(d => d.Identity)
                    .WithMany(p => p.tbl_PrivateKeys)
                    .HasForeignKey(d => d.IdentityId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_tbl_PrivateKey_IdentityID");
            });

            modelBuilder.Entity<tbl_PublicKey>(entity =>
            {
                entity.ToTable("tbl_PublicKey");

                entity.HasIndex(e => e.Id, "IX_tbl_PublicKey")
                    .IsUnique();

                entity.Property(e => e.Id).ValueGeneratedNever();

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

                entity.HasOne(d => d.Identity)
                    .WithMany(p => p.tbl_PublicKeys)
                    .HasForeignKey(d => d.IdentityId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_tbl_PublicKey_IdentityID");

                entity.HasOne(d => d.PrivateKey)
                    .WithMany(p => p.tbl_PublicKeys)
                    .HasForeignKey(d => d.PrivateKeyId)
                    .HasConstraintName("FK_tbl_PublicKey_PrivateKeyID");
            });

            modelBuilder.Entity<tbl_Setting>(entity =>
            {
                entity.ToTable("tbl_Setting");

                entity.HasIndex(e => e.Id, "IX_tbl_Settings")
                    .IsUnique();

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.ConfigKey)
                    .IsRequired()
                    .HasMaxLength(128)
                    .IsUnicode(false);

                entity.Property(e => e.ConfigValue)
                    .IsRequired()
                    .HasMaxLength(256)
                    .IsUnicode(false);

                entity.HasOne(d => d.Identity)
                    .WithMany(p => p.tbl_Settings)
                    .HasForeignKey(d => d.IdentityId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_tbl_Setting_IdentityID");
            });

            modelBuilder.Entity<tbl_User>(entity =>
            {
                entity.HasKey(e => e.IdentityId)
                    .HasName("PK_tbl_Users");

                entity.ToTable("tbl_User");

                entity.HasIndex(e => e.IdentityId, "IX_tbl_Users")
                    .IsUnique();

                entity.Property(e => e.IdentityId).ValueGeneratedNever();

                entity.Property(e => e.DebugLevel)
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

            modelBuilder.Entity<tbl_UserFile>(entity =>
            {
                entity.ToTable("tbl_UserFile");

                entity.HasIndex(e => e.Id, "IX_tbl_UserFiles")
                    .IsUnique();

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.HashSHA256).HasMaxLength(64);

                entity.Property(e => e.RealFileName).IsRequired();

                entity.Property(e => e.RealPath).IsRequired();

                entity.Property(e => e.VirtualName).IsRequired();

                entity.HasOne(d => d.Folder)
                    .WithMany(p => p.tbl_UserFiles)
                    .HasForeignKey(d => d.FolderId)
                    .HasConstraintName("FK_tbl_UserFile_FolderID");

                entity.HasOne(d => d.Identity)
                    .WithMany(p => p.tbl_UserFiles)
                    .HasForeignKey(d => d.IdentityId)
                    .HasConstraintName("FK_tbl_UserFile_IdentityID");
            });

            modelBuilder.Entity<tbl_UserFolder>(entity =>
            {
                entity.ToTable("tbl_UserFolder");

                entity.HasIndex(e => e.Id, "IX_tbl_UserFolders")
                    .IsUnique();

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.VirtualName)
                    .IsRequired()
                    .IsUnicode(false);

                entity.HasOne(d => d.Identity)
                    .WithMany(p => p.tbl_UserFolders)
                    .HasForeignKey(d => d.IdentityId)
                    .HasConstraintName("FK_tbl_UserFolder_IdentityID");

                entity.HasOne(d => d.Parent)
                    .WithMany(p => p.InverseParent)
                    .HasForeignKey(d => d.ParentId)
                    .HasConstraintName("FK_tbl_UserFolder_ParentID");
            });

            modelBuilder.Entity<tbl_UserMount>(entity =>
            {
                entity.HasKey(e => e.IdentityId);

                entity.ToTable("tbl_UserMount");

                entity.HasIndex(e => e.IdentityId, "IX_tbl_UserMounts")
                    .IsUnique();

                entity.Property(e => e.IdentityId).ValueGeneratedNever();

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

                entity.HasOne(d => d.Credential)
                    .WithMany(p => p.tbl_UserMounts)
                    .HasForeignKey(d => d.CredentialId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_tbl_UserMount_CredentialID");

                entity.HasOne(d => d.Identity)
                    .WithOne(p => p.tbl_UserMount)
                    .HasForeignKey<tbl_UserMount>(d => d.IdentityId)
                    .HasConstraintName("FK_tbl_UserMount_IdentityID");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
