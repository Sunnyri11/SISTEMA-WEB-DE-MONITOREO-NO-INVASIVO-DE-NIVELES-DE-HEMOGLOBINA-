using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Scaffolding.Internal;

namespace SistemaHBAPI.Models;

public partial class SistemaHBContext : DbContext
{
    public SistemaHBContext()
    {
    }

    public SistemaHBContext(DbContextOptions<SistemaHBContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Administrador> Administradors { get; set; }

    public virtual DbSet<Ciudad> Ciudads { get; set; }

    public virtual DbSet<CorreosElectronico> CorreosElectronicos { get; set; }

    public virtual DbSet<Departamento> Departamentos { get; set; }

    public virtual DbSet<Especialidade> Especialidades { get; set; }

    public virtual DbSet<FechaNacimiento> FechaNacimientos { get; set; }

    public virtual DbSet<Genero> Generos { get; set; }

    public virtual DbSet<Medico> Medicos { get; set; }

    public virtual DbSet<MedicoEspecialidad> MedicoEspecialidads { get; set; }

    public virtual DbSet<NivelHemoglobina> NivelHemoglobinas { get; set; }

    public virtual DbSet<PacienteAdministrador> PacienteAdministradors { get; set; }

    public virtual DbSet<PacienteMedico> PacienteMedicos { get; set; }

    public virtual DbSet<Persona> Personas { get; set; }

    public virtual DbSet<RangoHemoglobina> RangoHemoglobinas { get; set; }

    public virtual DbSet<TipoSangre> TipoSangres { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

//    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
//#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
//        => optionsBuilder.UseMySql("server=localhost;port=3307;database=sistemahb;uid=root", Microsoft.EntityFrameworkCore.ServerVersion.Parse("10.4.32-mariadb"));

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_general_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<Administrador>(entity =>
        {
            entity.HasKey(e => e.IdAdministrador).HasName("PRIMARY");

            entity.ToTable("administrador");

            entity.HasIndex(e => e.IdPersona, "fk_persona_administrador");

            entity.Property(e => e.IdAdministrador)
                .HasColumnType("int(11)")
                .HasColumnName("id_administrador");
            entity.Property(e => e.IdPersona)
                .HasColumnType("int(11)")
                .HasColumnName("id_persona");

            entity.HasOne(d => d.IdPersonaNavigation).WithMany(p => p.Administradors)
                .HasForeignKey(d => d.IdPersona)
                .HasConstraintName("fk_persona_administrador");
        });

        modelBuilder.Entity<Ciudad>(entity =>
        {
            entity.HasKey(e => e.IdCiudad).HasName("PRIMARY");

            entity.ToTable("ciudad");

            entity.HasIndex(e => e.IdDepartamento, "fk_departamento_ciudad");

            entity.Property(e => e.IdCiudad)
                .HasColumnType("int(11)")
                .HasColumnName("id_ciudad");
            entity.Property(e => e.Altitud)
                .HasPrecision(10, 2)
                .HasColumnName("altitud");
            entity.Property(e => e.IdDepartamento)
                .HasColumnType("int(11)")
                .HasColumnName("id_departamento");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .HasColumnName("nombre");

            entity.HasOne(d => d.IdDepartamentoNavigation).WithMany(p => p.Ciudads)
                .HasForeignKey(d => d.IdDepartamento)
                .HasConstraintName("fk_departamento_ciudad");
        });

        modelBuilder.Entity<CorreosElectronico>(entity =>
        {
            entity.HasKey(e => e.IdCorreoElectronico).HasName("PRIMARY");

            entity.ToTable("correos_electronicos");

            entity.HasIndex(e => e.CorreoElectronico, "correo_electronico").IsUnique();

            entity.Property(e => e.IdCorreoElectronico)
                .HasColumnType("int(11)")
                .HasColumnName("id_correo_electronico");
            entity.Property(e => e.CorreoElectronico)
                .HasColumnType("text")
                .HasColumnName("correo_electronico");
            entity.Property(e => e.Contraseña).HasColumnType("text").HasColumnName("contraseña");
        });

        modelBuilder.Entity<Departamento>(entity =>
        {
            entity.HasKey(e => e.IdDepartamento).HasName("PRIMARY");

            entity.ToTable("departamentos");

            entity.Property(e => e.IdDepartamento)
                .HasColumnType("int(11)")
                .HasColumnName("id_departamento");
            entity.Property(e => e.Departamento1)
                .HasMaxLength(200)
                .HasColumnName("departamento");
        });

        modelBuilder.Entity<Especialidade>(entity =>
        {
            entity.HasKey(e => e.IdEspecialidad).HasName("PRIMARY");

            entity.ToTable("especialidades");

            entity.Property(e => e.IdEspecialidad)
                .ValueGeneratedNever()
                .HasColumnType("int(11)")
                .HasColumnName("id_especialidad");
            entity.Property(e => e.Nombre)
                .HasMaxLength(200)
                .HasColumnName("nombre");
        });

        modelBuilder.Entity<FechaNacimiento>(entity =>
        {
            entity.HasKey(e => e.IdFechaNacimiento).HasName("PRIMARY");

            entity.ToTable("fecha_nacimiento");

            entity.Property(e => e.IdFechaNacimiento)
                .HasColumnType("int(11)")
                .HasColumnName("id_fecha_nacimiento");
            entity.Property(e => e.FechaDeNacimiento).HasColumnName("fecha_de_nacimiento");
        });

        modelBuilder.Entity<Genero>(entity =>
        {
            entity.HasKey(e => e.IdGenero).HasName("PRIMARY");

            entity.ToTable("genero");

            entity.HasIndex(e => e.Nombre, "nombre").IsUnique();

            entity.Property(e => e.IdGenero)
                .HasColumnType("int(11)")
                .HasColumnName("id_genero");
            entity.Property(e => e.Nombre)
                .HasColumnType("enum('Masculino','Femenino')")
                .HasColumnName("nombre");
        });

        modelBuilder.Entity<Medico>(entity =>
        {
            entity.HasKey(e => e.IdMedico).HasName("PRIMARY");

            entity.ToTable("medico");

            entity.HasIndex(e => e.IdPersona, "fk_medico_persona");

            entity.Property(e => e.IdMedico)
                .HasColumnType("int(11)")
                .HasColumnName("id_medico");
            entity.Property(e => e.IdPersona)
                .HasColumnType("int(11)")
                .HasColumnName("id_persona");

            entity.HasOne(d => d.IdPersonaNavigation).WithMany(p => p.Medicos)
                .HasForeignKey(d => d.IdPersona)
                .HasConstraintName("fk_medico_persona");
        });

        modelBuilder.Entity<MedicoEspecialidad>(entity =>
        {
            entity.HasKey(e => e.IdMedicoEspecialidad).HasName("PRIMARY");

            entity.ToTable("medico_especialidad");

            entity.HasIndex(e => e.IdEspecialidad, "fk_especialidad_medico");

            entity.HasIndex(e => e.IdMedico, "fk_medico_especialidad_");

            entity.Property(e => e.IdMedicoEspecialidad)
                .HasColumnType("int(11)")
                .HasColumnName("id_medico_especialidad");
            entity.Property(e => e.IdEspecialidad)
                .HasColumnType("int(11)")
                .HasColumnName("id_especialidad");
            entity.Property(e => e.IdMedico)
                .HasColumnType("int(11)")
                .HasColumnName("id_medico");

            entity.HasOne(d => d.IdEspecialidadNavigation).WithMany(p => p.MedicoEspecialidads)
                .HasForeignKey(d => d.IdEspecialidad)
                .HasConstraintName("fk_especialidad_medico");

            entity.HasOne(d => d.IdMedicoNavigation).WithMany(p => p.MedicoEspecialidads)
                .HasForeignKey(d => d.IdMedico)
                .HasConstraintName("fk_medico_especialidad_");
        });

        modelBuilder.Entity<NivelHemoglobina>(entity =>
        {
            entity.HasKey(e => e.IdNivel).HasName("PRIMARY");

            entity.ToTable("nivel_hemoglobina");

            entity.HasIndex(e => e.IdUsuario, "fk_nivel_hemoglobina_paciente");

            entity.HasIndex(e => e.IdRangoHemoglobina, "id_rango_hemoglobina");

            entity.Property(e => e.IdNivel)
                .HasColumnType("int(11)")
                .HasColumnName("id_nivel");
            entity.Property(e => e.FechaAnalisis).HasColumnName("fecha_analisis");
            entity.Property(e => e.IdRangoHemoglobina)
                .HasColumnType("int(11)")
                .HasColumnName("id_rango_hemoglobina");
            entity.Property(e => e.IdUsuario)
                .HasColumnType("int(11)")
                .HasColumnName("id_usuario");
            entity.Property(e => e.ValorHemoglobina)
                .HasPrecision(4, 2)
                .HasColumnName("valor_hemoglobina");

            entity.HasOne(d => d.IdRangoHemoglobinaNavigation).WithMany(p => p.NivelHemoglobinas)
                .HasForeignKey(d => d.IdRangoHemoglobina)
                .HasConstraintName("nivel_hemoglobina_ibfk_1");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.NivelHemoglobinas)
                .HasForeignKey(d => d.IdUsuario)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_nivel_hemoglobina_paciente");
        });

        modelBuilder.Entity<PacienteAdministrador>(entity =>
        {
            entity.HasKey(e => e.IdUsuarioAdministrador).HasName("PRIMARY");

            entity.ToTable("paciente_administrador");

            entity.HasIndex(e => e.IdAdministrador, "fk_administrador_usuario");

            entity.HasIndex(e => e.IdUsuario, "fk_paciente_administrador");

            entity.Property(e => e.IdUsuarioAdministrador)
                .HasColumnType("int(11)")
                .HasColumnName("id_usuario_administrador");
            entity.Property(e => e.IdAdministrador)
                .HasColumnType("int(11)")
                .HasColumnName("id_administrador");
            entity.Property(e => e.IdUsuario)
                .HasColumnType("int(11)")
                .HasColumnName("id_usuario");

            entity.HasOne(d => d.IdAdministradorNavigation).WithMany(p => p.PacienteAdministradors)
                .HasForeignKey(d => d.IdAdministrador)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_administrador_usuario");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.PacienteAdministradors)
                .HasForeignKey(d => d.IdUsuario)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_paciente_administrador");
        });

        modelBuilder.Entity<PacienteMedico>(entity =>
        {
            entity.HasKey(e => e.IdUsuarioMedico).HasName("PRIMARY");

            entity.ToTable("paciente_medico");

            entity.HasIndex(e => e.IdMedico, "fk_medico_usuario");

            entity.HasIndex(e => e.IdUsuario, "fk_paciente_medico");

            entity.Property(e => e.IdUsuarioMedico)
                .HasColumnType("int(11)")
                .HasColumnName("id_usuario_medico");
            entity.Property(e => e.IdMedico)
                .HasColumnType("int(11)")
                .HasColumnName("id_medico");
            entity.Property(e => e.IdUsuario)
                .HasColumnType("int(11)")
                .HasColumnName("id_usuario");

            entity.HasOne(d => d.IdMedicoNavigation).WithMany(p => p.PacienteMedicos)
                .HasForeignKey(d => d.IdMedico)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_medico_usuario");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.PacienteMedicos)
                .HasForeignKey(d => d.IdUsuario)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_paciente_medico");
        });

        modelBuilder.Entity<Persona>(entity =>
        {
            entity.HasKey(e => e.IdPersona).HasName("PRIMARY");

            entity.ToTable("persona");

            entity.HasIndex(e => e.IdCorreoElectronico, "fk_correo_persona");

            entity.HasIndex(e => e.IdFechaNacimiento, "fk_fecha_persona");

            entity.HasIndex(e => e.IdGenero, "id_genero");

            entity.Property(e => e.IdPersona)
                .HasColumnType("int(11)")
                .HasColumnName("id_persona");
            entity.Property(e => e.Apellido)
                .HasMaxLength(100)
                .HasColumnName("apellido");
            entity.Property(e => e.IdCorreoElectronico)
                .HasColumnType("int(11)")
                .HasColumnName("id_correo_electronico");
            entity.Property(e => e.IdFechaNacimiento)
                .HasColumnType("int(11)")
                .HasColumnName("id_fecha_nacimiento");
            entity.Property(e => e.IdGenero)
                .HasColumnType("int(11)")
                .HasColumnName("id_genero");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .HasColumnName("nombre");

            entity.HasOne(d => d.IdCorreoElectronicoNavigation).WithMany(p => p.Personas)
                .HasForeignKey(d => d.IdCorreoElectronico)
                .HasConstraintName("fk_correo_persona");

            entity.HasOne(d => d.IdFechaNacimientoNavigation).WithMany(p => p.Personas)
                .HasForeignKey(d => d.IdFechaNacimiento)
                .HasConstraintName("fk_fecha_persona");

            entity.HasOne(d => d.IdGeneroNavigation).WithMany(p => p.Personas)
                .HasForeignKey(d => d.IdGenero)
                .HasConstraintName("persona_ibfk_1");
        });

        modelBuilder.Entity<RangoHemoglobina>(entity =>
        {
            entity.HasKey(e => e.IdRango).HasName("PRIMARY");

            entity.ToTable("rango_hemoglobina");

            entity.HasIndex(e => e.IdCiudad, "id_ciudad");

            entity.HasIndex(e => e.IdGenero, "id_genero");

            entity.Property(e => e.IdRango)
                .HasColumnType("int(11)")
                .HasColumnName("id_rango");
            entity.Property(e => e.IdCiudad)
                .HasColumnType("int(11)")
                .HasColumnName("id_ciudad");
            entity.Property(e => e.IdGenero)
                .HasColumnType("int(11)")
                .HasColumnName("id_genero");
            entity.Property(e => e.ValorMax)
                .HasPrecision(4, 2)
                .HasColumnName("valor_max");
            entity.Property(e => e.ValorMin)
                .HasPrecision(4, 2)
                .HasColumnName("valor_min");

            entity.HasOne(d => d.IdCiudadNavigation).WithMany(p => p.RangoHemoglobinas)
                .HasForeignKey(d => d.IdCiudad)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("rango_hemoglobina_ibfk_1");

            entity.HasOne(d => d.IdGeneroNavigation).WithMany(p => p.RangoHemoglobinas)
                .HasForeignKey(d => d.IdGenero)
                .HasConstraintName("rango_hemoglobina_ibfk_2");
        });

        modelBuilder.Entity<TipoSangre>(entity =>
        {
            entity.HasKey(e => e.IdTipoSangre).HasName("PRIMARY");

            entity.ToTable("tipo_sangre");

            entity.Property(e => e.IdTipoSangre)
                .HasColumnType("int(11)")
                .HasColumnName("id_tipo_sangre");
            entity.Property(e => e.TipoDeSangre)
                .HasMaxLength(100)
                .HasColumnName("tipo_de_sangre");
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.IdUsuario).HasName("PRIMARY");

            entity.ToTable("usuario");

            entity.HasIndex(e => e.IdPersona, "fk_persona_paciente");

            entity.HasIndex(e => e.IdTipoSangre, "fk_tipo_sangre_paciente");

            entity.Property(e => e.IdUsuario)
                .HasColumnType("int(11)")
                .HasColumnName("id_usuario");
            entity.Property(e => e.IdPersona)
                .HasColumnType("int(11)")
                .HasColumnName("id_persona");
            entity.Property(e => e.IdTipoSangre)
                .HasColumnType("int(11)")
                .HasColumnName("id_tipo_sangre");

            entity.HasOne(d => d.IdPersonaNavigation).WithMany(p => p.Usuarios)
                .HasForeignKey(d => d.IdPersona)
                .HasConstraintName("fk_persona_paciente");

            entity.HasOne(d => d.IdTipoSangreNavigation).WithMany(p => p.Usuarios)
                .HasForeignKey(d => d.IdTipoSangre)
                .HasConstraintName("fk_tipo_sangre_paciente");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
