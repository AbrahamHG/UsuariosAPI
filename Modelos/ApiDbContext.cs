using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace UsuariosAPI.Modelos;

public partial class ApiDbContext : DbContext
{
    public ApiDbContext()
    {
    }

    public ApiDbContext(DbContextOptions<ApiDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Log> Logs { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Log>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Logs__3214EC27BEF9F702");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Accion)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.Cambios)
                .HasMaxLength(40)
                .IsUnicode(false);
            entity.Property(e => e.Fecha).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.RealizadoPor)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.UsuarioId).HasColumnName("UsuarioID");

            entity.HasOne(d => d.Usuario).WithMany(p => p.Logs)
                .HasForeignKey(d => d.UsuarioId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK__Logs__UsuarioID__4E88ABD4");


        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Usuario__3214EC072B6BEA05");

            entity.ToTable("Usuario");

            entity.Property(e => e.Email)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.FechaCreacion).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Nombre)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Rol)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("Usuario");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
