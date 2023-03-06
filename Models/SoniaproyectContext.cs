using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace ProyectoSonia.Models;

public partial class SoniaproyectContext : DbContext
{
    public SoniaproyectContext()
    {
    }

    public SoniaproyectContext(DbContextOptions<SoniaproyectContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Archivo> Archivos { get; set; }

    public virtual DbSet<Area> Areas { get; set; }

    public virtual DbSet<Dropboxkey> Dropboxkeys { get; set; }

    public virtual DbSet<Imagene> Imagenes { get; set; }

    public virtual DbSet<Informe> Informes { get; set; }

    public virtual DbSet<PalabrasClave> PalabrasClaves { get; set; }

    public virtual DbSet<PalabrasClaveDescripcion> PalabrasClaveDescripcions { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseMySql("server=aws.connect.psdb.cloud;port=3306;database=soniaproyect;uid=su0xfvdvjetsy1szpv1u;password=pscale_pw_8mNbMFDLCUggGX2JX6RFpaQdOANHvVGBbZSUtTC47zj", Microsoft.EntityFrameworkCore.ServerVersion.Parse("8.0.23-mysql"));

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_0900_ai_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<Archivo>(entity =>
        {
            entity.HasKey(e => e.IdArchivos).HasName("PRIMARY");

            entity.Property(e => e.IdArchivos).HasColumnName("idArchivos");
            entity.Property(e => e.IdInformes).HasColumnName("idInformes");
            entity.Property(e => e.Link)
                .HasMaxLength(805)
                .HasColumnName("link");
            entity.Property(e => e.Nombre).HasMaxLength(425);
        });

        modelBuilder.Entity<Area>(entity =>
        {
            entity.HasKey(e => e.IdAreas).HasName("PRIMARY");

            entity.Property(e => e.IdAreas).HasColumnName("idAreas");
            entity.Property(e => e.IdGrupoAreas)
                .HasMaxLength(45)
                .HasColumnName("idGrupoAreas");
            entity.Property(e => e.Nombre)
                .HasMaxLength(45)
                .HasColumnName("nombre");
            entity.Property(e => e.TituloWord).HasMaxLength(100);
        });

        modelBuilder.Entity<Dropboxkey>(entity =>
        {
            entity.HasKey(e => e.Iddropboxkey).HasName("PRIMARY");

            entity.ToTable("dropboxkey");

            entity.Property(e => e.Iddropboxkey).HasColumnName("iddropboxkey");
            entity.Property(e => e.Dropboxkeycol)
                .HasMaxLength(885)
                .HasColumnName("dropboxkeycol");
        });

        modelBuilder.Entity<Imagene>(entity =>
        {
            entity.HasKey(e => e.IdImagenes).HasName("PRIMARY");

            entity.Property(e => e.IdImagenes).HasColumnName("idImagenes");
            entity.Property(e => e.Fecha)
                .HasColumnType("datetime")
                .HasColumnName("fecha");
            entity.Property(e => e.IdAreas).HasColumnName("idAreas");
            entity.Property(e => e.IdInformes).HasColumnName("idInformes");
            entity.Property(e => e.Link)
                .HasMaxLength(800)
                .HasColumnName("link");
            entity.Property(e => e.Medidacorrectiva)
                .HasMaxLength(800)
                .HasColumnName("medidacorrectiva");
            entity.Property(e => e.Nombre)
                .HasMaxLength(455)
                .HasColumnName("nombre");
            entity.Property(e => e.TituloImagen)
                .HasMaxLength(450)
                .HasColumnName("tituloImagen");
        });

        modelBuilder.Entity<Informe>(entity =>
        {
            entity.HasKey(e => e.IdInformes).HasName("PRIMARY");

            entity.Property(e => e.IdInformes).HasColumnName("idInformes");
            entity.Property(e => e.Fecha)
                .HasColumnType("datetime")
                .HasColumnName("fecha");
            entity.Property(e => e.FechaFinal)
                .HasColumnType("datetime")
                .HasColumnName("fechaFinal");
            entity.Property(e => e.FechaInicial)
                .HasColumnType("datetime")
                .HasColumnName("fechaInicial");
            entity.Property(e => e.LinkGenerated)
                .HasDefaultValueSql("'0'")
                .HasColumnName("linkGenerated");
        });

        modelBuilder.Entity<PalabrasClave>(entity =>
        {
            entity.HasKey(e => e.IdPalabrasClave).HasName("PRIMARY");

            entity.ToTable("PalabrasClave");

            entity.Property(e => e.IdPalabrasClave).HasColumnName("idPalabrasClave");
            entity.Property(e => e.Nombre)
                .HasMaxLength(45)
                .HasColumnName("nombre");
        });

        modelBuilder.Entity<PalabrasClaveDescripcion>(entity =>
        {
            entity.HasKey(e => e.IdPalabrasClaveDescripcion).HasName("PRIMARY");

            entity.ToTable("PalabrasClaveDescripcion");

            entity.Property(e => e.IdPalabrasClaveDescripcion).HasColumnName("idPalabrasClaveDescripcion");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(900)
                .HasColumnName("descripcion");
            entity.Property(e => e.IdPalabrasClave).HasColumnName("idPalabrasClave");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
