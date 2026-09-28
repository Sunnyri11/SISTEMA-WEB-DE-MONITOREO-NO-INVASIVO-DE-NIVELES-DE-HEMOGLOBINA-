
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SistemaHBAPI.Models;

namespace SistemaHBAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AutenticacionController : ControllerBase
    {
        private readonly SistemaHBContext _context;

        public AutenticacionController(SistemaHBContext context)
        {
            _context = context;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LOGINFORMCLS model)
        {
            // 1. Buscar el registro del correo electrónico en la base de datos
            var correoReg = await _context.CorreosElectronicos
                .Include(c => c.Personas)
                    .ThenInclude(p => p.Usuarios)
                .Include(c => c.Personas)
                    .ThenInclude(p => p.Medicos)
                .Include(c => c.Personas)
                    .ThenInclude(p => p.Administradors)
                .FirstOrDefaultAsync(c => c.CorreoElectronico == model.Email);

            if (correoReg == null)
            {
                return Unauthorized(new { mensaje = "El correo electrónico no se encuentra registrado." });
            }

            // 2. Extraer la persona vinculada al correo
            var persona = correoReg.Personas.FirstOrDefault();
            if (persona == null)
            {
                return Unauthorized(new { mensaje = "No existe un perfil de persona asociado a este correo." });
            }

            // 3. VALIDACIÓN DE CONTRASEÑA E IDENTIDAD DE ROL
            // Nota: Aquí validamos el Apellido como contraseña temporal plana (como en tu estructura inicial). 
            // Recuerda migrar esto a un campo Password real con Hash en el futuro.
            if (correoReg.Contraseña != model.Password)
            {
                return Unauthorized(new { mensaje = "Contraseña incorrecta." });
            }

            bool tieneRolValido = false;
            string idEntidadRol = "0";

            switch (model.Rol.ToLower())
            {
                case "paciente":
                    var usuario = persona.Usuarios.FirstOrDefault();
                    if (usuario != null) { tieneRolValido = true; idEntidadRol = usuario.IdUsuario.ToString(); }
                    break;
                case "medico":
                    var medico = persona.Medicos.FirstOrDefault();
                    if (medico != null) { tieneRolValido = true; idEntidadRol = medico.IdMedico.ToString(); }
                    break;
                case "administrador":
                    var admin = persona.Administradors.FirstOrDefault();
                    if (admin != null) { tieneRolValido = true; idEntidadRol = admin.IdAdministrador.ToString(); }
                    break;
            }

            if (!tieneRolValido)
            {
                return Unauthorized(new { mensaje = $"El usuario no tiene asignado el rol de {model.Rol}." });
            }

            // 4. GENERAR EL TOKEN JWT CON LAS CONFIGURACIONES EXACTAS DE TU PROGRAM.CS
            var secretKey = "TuClaveSecretaSuperSeguraDeAlMenos32Bytes!"; // Debe ser idéntica a la del Program.cs
            var keyBytes = Encoding.UTF8.GetBytes(secretKey);

            var claims = new ClaimsIdentity();
            claims.AddClaim(new Claim(ClaimTypes.NameIdentifier, persona.IdPersona.ToString()));
            claims.AddClaim(new Claim(ClaimTypes.Email, model.Email));
            claims.AddClaim(new Claim(ClaimTypes.Role, model.Rol));
            claims.AddClaim(new Claim("IdEntidadRol", idEntidadRol)); // Guarda el ID específico de la tabla final

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = claims,
                Expires = DateTime.UtcNow.AddHours(4), // Duración del Token
                Issuer = "https://localhost:7176",
                Audience = "SistemaHBAutenticacionApi",
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(keyBytes), SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenConfig = tokenHandler.CreateToken(tokenDescriptor);
            var tokenCreado = tokenHandler.WriteToken(tokenConfig);

            // 5. Devolver respuesta exitosa con el token y datos básicos
            return Ok(new
            {
                token = tokenCreado,
                nombre = persona.Nombre,
                rol = model.Rol,
                idEntidad = idEntidadRol
            });
        }
    }
}
