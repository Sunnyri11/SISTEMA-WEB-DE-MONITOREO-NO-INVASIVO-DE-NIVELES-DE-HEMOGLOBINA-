using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaHBAPI.Models;

namespace SistemaHBAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EspecialidadesController : Controller
    {
        public readonly SistemaHBContext _context;
        public EspecialidadesController(SistemaHBContext _context)
        {
            this._context = _context;
        }
        [HttpGet()]
        public async Task<IActionResult> GetRoles()
        {
            var especialidades = await _context.Especialidades.Select(d => new { d.IdEspecialidad, d.Nombre }).ToListAsync();
            return Ok(especialidades);
        }
    }
}
