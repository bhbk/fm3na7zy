using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

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

        public virtual DbSet<tbl_Credential> tbl_Credential { get; set; }
        public virtual DbSet<tbl_Network> tbl_Network { get; set; }
        public virtual DbSet<tbl_PrivateKey> tbl_PrivateKey { get; set; }
        public virtual DbSet<tbl_PublicKey> tbl_PublicKey { get; set; }
        public virtual DbSet<tbl_Setting> tbl_Setting { get; set; }
        public virtual DbSet<tbl_User> tbl_User { get; set; }
        public virtual DbSet<tbl_UserFile> tbl_UserFile { get; set; }
        public virtual DbSet<tbl_UserFolder> tbl_UserFolder { get; set; }
        public virtual DbSet<tbl_UserMount> tbl_UserMount { get; set; }

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
            modelBuilder.Entity<tbl_Credential>(entity =>
            {
                entity.HasIndex(e => e.Id)
                    .HasName("IX_tbl_SysCredentials")
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
                entity.HasIndex(e => e.Id)
                    .HasName("IX_tbl_Networks")
                    .IsUnique();

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Action)
                    .IsRequired()
                    .HasMaxLength(8);

                entity.Property(e => e.Address)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.HasOne(d => d.Identity)
                    .WithMany(p => p.tbl_Network)
                    .HasForeignKey(d => d.IdentityId)
                    .HasConstraintName("FK_tbl_Network_IdentityID");
            });

            modelBuilder.Entity<tbl_PrivateKey>(entity =>
            {
                entity.HasIndex(e => e.Id)
                    .HasName("IX_tbl_UserPrivateKeys")
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
                    .WithMany(p => p.tbl_PrivateKey)
                    .HasForeignKey(d => d.IdentityId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_tbl_PrivateKey_IdentityID");
            });

            modelBuilder.Entity<tbl_PublicKey>(entity =>
            {
                entity.HasIndex(e => e.Id)
                    .HasName("IX_tbl_PublicKey")
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
                    .WithMany(p => p.tbl_PublicKey)
                    .HasForeignKey(d => d.IdentityId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_tbl_PublicKey_IdentityID");

                entity.HasOne(d => d.PrivateKey)
                    .WithMany(p => p.tbl_PublicKey)
                    .HasForeignKey(d => d.PrivateKeyId)
                    .HasConstraintName("FK_tbl_PublicKey_PrivateKeyID");
            });

            modelBuilder.Entity<tbl_Setting>(entity =>
            {
                entity.HasIndex(e => e.Id)
                    .HasName("IX_tbl_Settings")
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
                    .WithMany(p => p.tbl_Setting)
                    .HasForeignKey(d => d.IdentityId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_tbl_Setting_IdentityID");
            });

            modelBuilder.Entity<tbl_User>(entity =>
            {
                entity.HasKey(e => e.IdentityId)
                    .HasName("PK_tbl_Users");

                entity.HasIndex(e => e.IdentityId)
                    .HasName("IX_tbl_Users")
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
                entity.HasIndex(e => e.Id)
                    .HasName("IX_tbl_UserFiles")
                    .IsUnique();

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.HashSHA256).HasMaxLength(64);

                entity.Property(e => e.RealFileName).IsRequired();

                entity.Property(e => e.RealPath).IsRequired();

                entity.Property(e => e.VirtualName).IsRequired();

                entity.HasOne(d => d.Folder)
                    .WithMany(p => p.tbl_UserFile)
                    .HasForeignKey(d => d.FolderId)
                    .HasConstraintName("FK_tbl_UserFile_FolderID");

                entity.HasOne(d => d.Identity)
                    .WithMany(p => p.tbl_UserFile)
                    .HasForeignKey(d => d.IdentityId)
                    .HasConstraintName("FK_tbl_UserFile_IdentityID");
            });

            modelBuilder.Entity<tbl_UserFolder>(entity =>
            {
                entity.HasIndex(e => e.Id)
                    .HasName("IX_tbl_UserFolders")
                    .IsUnique();

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.VirtualName)
                    .IsRequired()
                    .IsUnicode(false);

                entity.HasOne(d => d.Identity)
                    .WithMany(p => p.tbl_UserFolder)
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

                entity.HasIndex(e => e.IdentityId)
                    .HasName("IX_tbl_UserMounts")
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
                    .WithMany(p => p.tbl_UserMount)
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
