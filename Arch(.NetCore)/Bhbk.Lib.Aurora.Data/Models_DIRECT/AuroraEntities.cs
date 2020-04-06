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

        public virtual DbSet<tbl_Settings> tbl_Settings { get; set; }
        public virtual DbSet<tbl_SystemKeys> tbl_SystemKeys { get; set; }
        public virtual DbSet<tbl_UserFiles> tbl_UserFiles { get; set; }
        public virtual DbSet<tbl_UserFolders> tbl_UserFolders { get; set; }
        public virtual DbSet<tbl_UserPasswords> tbl_UserPasswords { get; set; }
        public virtual DbSet<tbl_UserPrivateKeys> tbl_UserPrivateKeys { get; set; }
        public virtual DbSet<tbl_UserPublicKeys> tbl_UserPublicKeys { get; set; }
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
            modelBuilder.Entity<tbl_Settings>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.ConfigKey)
                    .IsRequired()
                    .HasMaxLength(128)
                    .IsUnicode(false);

                entity.Property(e => e.ConfigValue)
                    .IsRequired()
                    .HasMaxLength(256)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<tbl_SystemKeys>(entity =>
            {
                entity.HasIndex(e => e.Id)
                    .HasName("IX_tbl_PrivateKeys")
                    .IsUnique();

                entity.Property(e => e.Id).ValueGeneratedNever();

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

            modelBuilder.Entity<tbl_UserFiles>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.FileHashSHA256)
                    .IsRequired()
                    .HasMaxLength(64);

                entity.Property(e => e.RealFileName).IsRequired();

                entity.Property(e => e.RealPath).IsRequired();

                entity.Property(e => e.VirtualName).IsRequired();

                entity.HasOne(d => d.Folder)
                    .WithMany(p => p.tbl_UserFiles)
                    .HasForeignKey(d => d.FolderId)
                    .HasConstraintName("FK_tbl_UserFiles_VirtualPathID");

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

            modelBuilder.Entity<tbl_UserPasswords>(entity =>
            {
                entity.HasKey(e => e.UserId);

                entity.HasIndex(e => e.UserId)
                    .HasName("IX_tbl_UserPasswords")
                    .IsUnique();

                entity.Property(e => e.UserId).ValueGeneratedNever();

                entity.Property(e => e.ConcurrencyStamp)
                    .IsRequired()
                    .HasMaxLength(1024);

                entity.Property(e => e.PasswordHashPBKDF2).HasMaxLength(2048);

                entity.Property(e => e.PasswordHashSHA256).HasMaxLength(2048);

                entity.Property(e => e.SecurityStamp)
                    .IsRequired()
                    .HasMaxLength(1024);

                entity.HasOne(d => d.User)
                    .WithOne(p => p.tbl_UserPasswords)
                    .HasForeignKey<tbl_UserPasswords>(d => d.UserId)
                    .HasConstraintName("FK_tbl_UserPasswords_UserID");
            });

            modelBuilder.Entity<tbl_UserPrivateKeys>(entity =>
            {
                entity.HasIndex(e => e.Id)
                    .HasName("IX_tbl_UserPrivateKeys")
                    .IsUnique();

                entity.HasIndex(e => e.PublicKeyId)
                    .HasName("IX_tbl_UserPrivateKeys_PublicKeyID")
                    .IsUnique();

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.KeyValueAlgo)
                    .IsRequired()
                    .HasMaxLength(16);

                entity.Property(e => e.KeyValueBase64).IsRequired();

                entity.Property(e => e.KeyValuePass)
                    .IsRequired()
                    .HasMaxLength(1024);

                entity.HasOne(d => d.User)
                    .WithMany(p => p.tbl_UserPrivateKeys)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK_tbl_UserPrivateKeys_UserID");
            });

            modelBuilder.Entity<tbl_UserPublicKeys>(entity =>
            {
                entity.HasIndex(e => e.Id)
                    .HasName("IX_tbl_UserPublicKeys")
                    .IsUnique();

                entity.Property(e => e.Id).ValueGeneratedNever();

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

                entity.HasOne(d => d.PrivateKey)
                    .WithMany(p => p.tbl_UserPublicKeys)
                    .HasForeignKey(d => d.PrivateKeyId)
                    .HasConstraintName("FK_tbl_UserPublicKeys_UserPrivateID");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.tbl_UserPublicKeys)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK_tbl_UserPublicKeys_UserID");
            });

            modelBuilder.Entity<tbl_Users>(entity =>
            {
                entity.HasIndex(e => e.Id)
                    .HasName("IX_tbl_Users")
                    .IsUnique();

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.UserName)
                    .HasMaxLength(128)
                    .IsUnicode(false);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
