using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaHBAPI.Models;

namespace SistemaHBAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GeografiaController : ControllerBase
    {
        private readonly SistemaHBContext _context;

        public GeografiaController(SistemaHBContext context)
        {
            _context = context;
        }

        // 1. Obtener todos los departamentos para cargar el primer SELECT
        [HttpGet("departamentos")]
        public async Task<IActionResult> GetDepartamentos()
        {
            var departamentos = await _context.Departamentos
                .Select(d => new { d.IdDepartamento, d.Departamento1 })
                .ToListAsync();
            return Ok(departamentos);
        }

        // 2. Obtener las provincias/ciudades filtradas por el departamento seleccionado
        [HttpGet("provincias/{idDepartamento}")]
        public async Task<IActionResult> GetProvincias(int idDepartamento)
        {
            var provincias = await _context.Ciudads
                .Where(c => c.IdDepartamento == idDepartamento)
                .Select(c => new { c.IdCiudad, c.Nombre, c.Altitud })
                .ToListAsync();
            return Ok(provincias);
        }
    }
}
