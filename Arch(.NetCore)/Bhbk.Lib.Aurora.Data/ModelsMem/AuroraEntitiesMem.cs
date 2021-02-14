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

        public virtual DbSet<FileMem> tbl_Files { get; set; }
        public virtual DbSet<FileSystemMem> tbl_FileSystems { get; set; }
        public virtual DbSet<FileSystemLoginMem> tbl_FileSystemLogins { get; set; }
        public virtual DbSet<FileSystemUsageMem> tbl_FileSystemUsages { get; set; }
        public virtual DbSet<FolderMem> tbl_Folders { get; set; }
        public virtual DbSet<LoginMem> tbl_Logins { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("Relational:Collation", "SQL_Latin1_General_CP1_CI_AS");

            modelBuilder.Entity<FileMem>(entity =>
            {
                entity.ToTable("tbl_File");

                entity.HasIndex(e => e.Id, "IX_tbl_File")
                    .IsUnique();

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.HashValue).HasMaxLength(64);

                entity.Property(e => e.VirtualName)
                    .IsRequired()
                    .HasMaxLength(260);

                entity.HasOne(d => d.Creator)
                    .WithMany(p => p.FilesCreated)
                    .HasForeignKey(d => d.CreatorId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_tbl_File_tbl_Login");

                entity.HasOne(d => d.FileSystem)
                    .WithMany(p => p.Files)
                    .HasForeignKey(d => d.FileSystemId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_tbl_File_tbl_FileSystem");

                entity.HasOne(d => d.Folder)
                    .WithMany(p => p.Files)
                    .HasForeignKey(d => d.FolderId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_tbl_File_tbl_Folder");
            });

            modelBuilder.Entity<FileSystemMem>(entity =>
            {
                entity.ToTable("tbl_FileSystem");

                entity.Property(e => e.Id).ValueGeneratedNever();
            });

            modelBuilder.Entity<FileSystemLoginMem>(entity =>
            {
                entity.HasKey(e => new { e.UserId, e.FileSystemId });

                entity.ToTable("tbl_FileSystemLogin");

                entity.HasIndex(e => new { e.UserId, e.FileSystemId }, "IX_tbl_FileSystemLogin")
                    .IsUnique();

                entity.HasOne(d => d.FileSystem)
                    .WithMany(p => p.Logins)
                    .HasForeignKey(d => d.FileSystemId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_tbl_FileSystemLogin_tbl_FileSystem");
            });

            modelBuilder.Entity<FileSystemUsageMem>(entity =>
            {
                entity.HasKey(e => e.FileSystemId);

                entity.ToTable("tbl_FileSystemUsage");

                entity.Property(e => e.FileSystemId).ValueGeneratedNever();

                entity.HasOne(d => d.FileSystem)
                    .WithOne(p => p.Usage)
                    .HasForeignKey<FileSystemUsageMem>(d => d.FileSystemId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_tbl_FileSystemUsage_tbl_FileSystem");
            });

            modelBuilder.Entity<FolderMem>(entity =>
            {
                entity.ToTable("tbl_Folder");

                entity.HasIndex(e => e.Id, "IX_tbl_Folder")
                    .IsUnique();

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.VirtualName).IsRequired();

                entity.HasOne(d => d.Creator)
                    .WithMany(p => p.FoldersCreated)
                    .HasForeignKey(d => d.CreatorId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_tbl_Folder_tbl_Login");

                entity.HasOne(d => d.FileSystem)
                    .WithMany(p => p.Folders)
                    .HasForeignKey(d => d.FileSystemId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_tbl_Folder_tbl_FileSystem");

                entity.HasOne(d => d.Parent)
                    .WithMany(p => p.Folders)
                    .HasForeignKey(d => d.ParentId)
                    .HasConstraintName("FK_tbl_Folder_tbl_Folder_Parent");
            });

            modelBuilder.Entity<LoginMem>(entity =>
            {
                entity.HasKey(e => e.UserId);

                entity.ToTable("tbl_Login");

                entity.HasIndex(e => e.UserId, "IX_tbl_Login")
                    .IsUnique();

                entity.Property(e => e.UserId).ValueGeneratedNever();

                entity.Property(e => e.UserName)
                    .IsRequired()
                    .HasMaxLength(128);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
