using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

#nullable disable

namespace Bhbk.Lib.Aurora.Data.ModelsMem
{
    public partial class AuroraEntitiesMem : DbContext
    {
        public AuroraEntitiesMem()
        {
        }

        public AuroraEntitiesMem(DbContextOptions<AuroraEntitiesMem> options)
            : base(options)
        {
        }

        public virtual DbSet<UserMem> UserMem { get; set; }
        public virtual DbSet<UserFileMem> UserFileMem { get; set; }
        public virtual DbSet<UserFolderMem> UserFolderMem { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("Relational:Collation", "SQL_Latin1_General_CP1_CI_AS");

            modelBuilder.Entity<UserMem>(entity =>
            {
                entity.HasKey(e => e.IdentityId)
                    .HasName("PK_UserMem");

                entity.ToTable("UserMem");

                entity.HasIndex(e => e.IdentityId, "IX_UserMem")
                    .IsUnique();

                entity.Property(e => e.IdentityId).ValueGeneratedNever();

                entity.Property(e => e.IdentityAlias)
                    .IsRequired()
                    .HasMaxLength(128)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<UserFileMem>(entity =>
            {
                entity.ToTable("UserFileMem");

                entity.HasIndex(e => e.Id, "IX_UserFileMem")
                    .IsUnique();

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.HashSHA256)
                    .IsRequired()
                    .HasMaxLength(64);

                entity.Property(e => e.VirtualName)
                    .IsRequired()
                    .HasMaxLength(260);

                entity.HasOne(d => d.Folder)
                    .WithMany(p => p.Files)
                    .HasForeignKey(d => d.FolderId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_UserFileMem_FolderID");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Files)
                    .HasForeignKey(d => d.IdentityId)
                    .HasConstraintName("FK_UserFileMem_IdentityID");
            });

            modelBuilder.Entity<UserFolderMem>(entity =>
            {
                entity.ToTable("UserFolderMem");

                entity.HasIndex(e => e.Id, "IX_UserFolderMem")
                    .IsUnique();

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.VirtualName)
                    .IsRequired()
                    .IsUnicode(false);

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Folders)
                    .HasForeignKey(d => d.IdentityId)
                    .HasConstraintName("FK_UserFolderMem_IdentityID");

                entity.HasOne(d => d.Parent)
                    .WithMany(p => p.Folders)
                    .HasForeignKey(d => d.ParentId)
                    .HasConstraintName("FK_UserFolderMem_ParentID");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
