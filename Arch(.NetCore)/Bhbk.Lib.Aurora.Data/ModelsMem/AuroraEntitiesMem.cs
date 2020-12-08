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

        public virtual DbSet<E_LoginMem> UserLoginMem { get; set; }
        public virtual DbSet<E_FileMem> UserFileMem { get; set; }
        public virtual DbSet<E_FolderMem> UserFolderMem { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("Relational:Collation", "SQL_Latin1_General_CP1_CI_AS");

            modelBuilder.Entity<E_FileMem>(entity =>
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
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK_UserFileMem_IdentityID");
            });

            modelBuilder.Entity<E_FolderMem>(entity =>
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
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK_UserFolderMem_IdentityID");

                entity.HasOne(d => d.Parent)
                    .WithMany(p => p.Folders)
                    .HasForeignKey(d => d.ParentId)
                    .HasConstraintName("FK_UserFolderMem_ParentID");
            });

            modelBuilder.Entity<E_LoginMem>(entity =>
            {
                entity.HasKey(e => e.UserId)
                    .HasName("PK_UserLoginMem");

                entity.ToTable("UserLoginMem");

                entity.HasIndex(e => e.UserId, "IX_UserLoginMem")
                    .IsUnique();

                entity.Property(e => e.UserId).ValueGeneratedNever();

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
