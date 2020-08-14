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

        public virtual DbSet<tbl_Credentials> tbl_Credentials { get; set; }
        public virtual DbSet<tbl_Networks> tbl_Networks { get; set; }
        public virtual DbSet<tbl_PrivateKeys> tbl_PrivateKeys { get; set; }
        public virtual DbSet<tbl_PublicKeys> tbl_PublicKeys { get; set; }
        public virtual DbSet<tbl_Settings> tbl_Settings { get; set; }
        public virtual DbSet<tbl_UserFiles> tbl_UserFiles { get; set; }
        public virtual DbSet<tbl_UserFolders> tbl_UserFolders { get; set; }
        public virtual DbSet<tbl_UserMounts> tbl_UserMounts { get; set; }
        public virtual DbSet<tbl_Users> tbl_Users { get; set; }

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
            modelBuilder.Entity<tbl_Credentials>(entity =>
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

            modelBuilder.Entity<tbl_Networks>(entity =>
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

                entity.HasOne(d => d.User)
                    .WithMany(p => p.tbl_Networks)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK_tbl_Networks_UserID");
            });

            modelBuilder.Entity<tbl_PrivateKeys>(entity =>
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

                entity.HasOne(d => d.User)
                    .WithMany(p => p.tbl_PrivateKeys)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_tbl_PrivateKeys_UserID");
            });

            modelBuilder.Entity<tbl_PublicKeys>(entity =>
            {
                entity.HasIndex(e => e.Id)
                    .HasName("IX_tbl_UserPublicKeys")
                    .IsUnique();

                entity.Property(e => e.Id).ValueGeneratedNever();

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

                entity.HasOne(d => d.PrivateKey)
                    .WithMany(p => p.tbl_PublicKeys)
                    .HasForeignKey(d => d.PrivateKeyId)
                    .HasConstraintName("FK_tbl_PublicKeys_PrivateKeyID");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.tbl_PublicKeys)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_tbl_PublicKeys_UserID");
            });

            modelBuilder.Entity<tbl_Settings>(entity =>
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

                entity.HasOne(d => d.User)
                    .WithMany(p => p.tbl_Settings)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_tbl_Settings_UserID");
            });

            modelBuilder.Entity<tbl_UserFiles>(entity =>
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
                    .WithMany(p => p.tbl_UserFiles)
                    .HasForeignKey(d => d.FolderId)
                    .HasConstraintName("FK_tbl_UserFiles_FolderID");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.tbl_UserFiles)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK_tbl_UserFiles_UserID");
            });

            modelBuilder.Entity<tbl_UserFolders>(entity =>
            {
                entity.HasIndex(e => e.Id)
                    .HasName("IX_tbl_UserFolders")
                    .IsUnique();

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.VirtualName)
                    .IsRequired()
                    .IsUnicode(false);

                entity.HasOne(d => d.Parent)
                    .WithMany(p => p.InverseParent)
                    .HasForeignKey(d => d.ParentId)
                    .HasConstraintName("FK_tbl_UserFolders_ParentID");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.tbl_UserFolders)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK_tbl_UserFolders_UserID");
            });

            modelBuilder.Entity<tbl_UserMounts>(entity =>
            {
                entity.HasKey(e => e.UserId);

                entity.HasIndex(e => e.UserId)
                    .HasName("IX_tbl_UserMounts")
                    .IsUnique();

                entity.Property(e => e.UserId).ValueGeneratedNever();

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
                    .HasConstraintName("FK_tbl_UserMounts_CredentialID");

                entity.HasOne(d => d.User)
                    .WithOne(p => p.tbl_UserMounts)
                    .HasForeignKey<tbl_UserMounts>(d => d.UserId)
                    .HasConstraintName("FK_tbl_UserMounts_UserID");
            });

            modelBuilder.Entity<tbl_Users>(entity =>
            {
                entity.HasIndex(e => e.Id)
                    .HasName("IX_tbl_Users")
                    .IsUnique();

                entity.Property(e => e.Id).ValueGeneratedNever();

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
