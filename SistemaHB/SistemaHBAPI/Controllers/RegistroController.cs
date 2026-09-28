using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaHBAPI.Models;
using System;
using System.Threading.Tasks;

namespace SistemaHBAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RegistroController : ControllerBase
    {
        private readonly SistemaHBContext _context;

        public RegistroController(SistemaHBContext context)
        {
            _context = context;
        }

        [HttpPost("usuario")]
        public async Task<IActionResult> RegistrarPaciente([FromBody] REGISTROFORMCLS model)
        {
            // 1. Validar si el correo electrónico ya existe en la base de datos
            var correoExiste = await _context.CorreosElectronicos
                .AnyAsync(c => c.CorreoElectronico == model.Correo);

            if (correoExiste)
            {
                return BadRequest(new { mensaje = "El correo electrónico ya se encuentra registrado por otro usuario." });
            }

            // Iniciamos una transacción para asegurar que se guarden todas las tablas o ninguna
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // 2. BUSCAR O INSERTAR LA FECHA DE NACIMIENTO
                var fechaReg = await _context.FechaNacimientos
                    .FirstOrDefaultAsync(f => f.FechaDeNacimiento == model.FechaNacimiento);

                if (fechaReg == null)
                {
                    fechaReg = new FechaNacimiento { FechaDeNacimiento = model.FechaNacimiento };
                    _context.FechaNacimientos.Add(fechaReg);
                    await _context.SaveChangesAsync();
                }

                // 3. BUSCAR O INSERTAR EL CORREO ELECTRÓNICO (Con tu nuevo campo Password)
                var nuevoCorreo = new CorreosElectronico
                {
                    CorreoElectronico = model.Correo,
                    Contraseña=model.Password
                };
                _context.CorreosElectronicos.Add(nuevoCorreo);
                await _context.SaveChangesAsync();

                // 4. VALIDAR EL GÉNERO
                // Intenta buscar por nombre, si no lo encuentra usa el ID 1 por defecto para no romper el flujo
                var generoReg = await _context.Generos
                    .FirstOrDefaultAsync(g => g.Nombre.ToLower() == model.Genero.ToLower());

                int idGeneroFinal = generoReg?.IdGenero ?? 1;

                // 5. REGISTRAR LA PERSONA (Unificando los apellidos del formulario)
                var nuevaPersona = new Persona
                {
                    Nombre = model.Nombre,
                    Apellido = $"{model.ApellidoPaterno} {model.ApellidoMaterno}".Trim(),
                    IdFechaNacimiento = fechaReg.IdFechaNacimiento,
                    IdCorreoElectronico = nuevoCorreo.IdCorreoElectronico,
                    IdGenero = idGeneroFinal
                };
                _context.Personas.Add(nuevaPersona);
                await _context.SaveChangesAsync();

                // 6. REGISTRAR EL USUARIO (PACIENTE) VINCULADO A LA PERSONA
                if (model.Rol == "Paciente")
                {
                    var nuevoUsuario = new Usuario
                    {
                        IdPersona = nuevaPersona.IdPersona,
                        IdTipoSangre = null
                    };
                    _context.Usuarios.Add(nuevoUsuario);
                    await _context.SaveChangesAsync();
                }
                else if(model.Rol=="Medico")
                {
                    var nuevoMedico = new Medico
                    {
                        IdPersona = nuevaPersona.IdPersona
                    };
                    _context.Medicos.Add(nuevoMedico);
                    await _context.SaveChangesAsync();

                    var idespecialidad = await _context.Especialidades.FirstOrDefaultAsync(c => c.IdEspecialidad == model.idEspecialidad);

                    var nuevoMedico_Especialidad = new MedicoEspecialidad { 
                        IdMedico=nuevoMedico.IdMedico,
                        IdEspecialidad=idespecialidad?.IdEspecialidad,
                    };

                    _context.MedicoEspecialidads.Add(nuevoMedico_Especialidad);
                    await _context.SaveChangesAsync();
                }

                // 7. MANEJO OPCIONAL DE CIUDAD / ALTITUD
                // Si tienes una tabla ciudad y quieres guardar la procedencia del paciente:
                var ciudadExiste = await _context.Ciudads
                    .FirstOrDefaultAsync(c => c.Nombre.ToLower() == model.Provincia.ToLower());

                if (ciudadExiste == null)
                {
                    var nuevaCiudad = new Ciudad
                    {
                        Nombre = model.Provincia,
                        Altitud = model.Altitud,
                        IdDepartamento = 1 // ID por defecto o mapeado de model.Departamento
                    };
                    _context.Ciudads.Add(nuevaCiudad);
                    await _context.SaveChangesAsync();
                }

                // Si todo salió bien, consolidamos los cambios en MySQL
                await transaction.CommitAsync();

                return Ok(new { mensaje = "¡Paciente registrado exitosamente en el sistema!" });
            }
            catch (Exception ex)
            {
                // Si algo falla, deshace todos los inserts parciales para no dejar datos corruptos
                await transaction.RollbackAsync();
                return StatusCode(500, new { mensaje = "Error interno al procesar el registro.", detalle = ex.Message });
            }
        }
    }
}
