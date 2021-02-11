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

        public virtual DbSet<FileMem_EF> FileMem { get; set; }
        public virtual DbSet<FolderMem_EF> FolderMem { get; set; }
        public virtual DbSet<LoginMem_EF> LoginMem { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("Relational:Collation", "SQL_Latin1_General_CP1_CI_AS");

            modelBuilder.Entity<FileMem_EF>(entity =>
            {
                entity.ToTable("FileMem");

                entity.HasIndex(e => e.Id, "IX_FileMem")
                    .IsUnique();

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.HashValue)
                    .IsRequired()
                    .HasMaxLength(64);

                entity.Property(e => e.VirtualName)
                    .IsRequired()
                    .HasMaxLength(260);

                entity.HasOne(d => d.Folder)
                    .WithMany(p => p.Files)
                    .HasForeignKey(d => d.FolderId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_FileMem_FolderID");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Files)
                    .HasForeignKey(d => d.CreatorId)
                    .HasConstraintName("FK_FileMem_IdentityID");
            });

            modelBuilder.Entity<FolderMem_EF>(entity =>
            {
                entity.ToTable("FolderMem");

                entity.HasIndex(e => e.Id, "IX_FolderMem")
                    .IsUnique();

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.VirtualName)
                    .IsRequired()
                    .IsUnicode(false);

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Folders)
                    .HasForeignKey(d => d.CreatorId)
                    .HasConstraintName("FK_FolderMem_IdentityID");

                entity.HasOne(d => d.Parent)
                    .WithMany(p => p.Folders)
                    .HasForeignKey(d => d.ParentId)
                    .HasConstraintName("FK_FolderMem_ParentID");
            });

            modelBuilder.Entity<LoginMem_EF>(entity =>
            {
                entity.HasKey(e => e.UserId)
                    .HasName("PK_LoginMem");

                entity.ToTable("LoginMem");

                entity.HasIndex(e => e.UserId, "IX_LoginMem")
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
