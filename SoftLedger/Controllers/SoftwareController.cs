using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SoftLedger.Data;
using SoftLedger.Models;
using Microsoft.AspNetCore.Authorization;

namespace SoftLedger.Controllers
{


    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class SoftwareController : ControllerBase
    {
        private readonly AppDbContext _context;

        public SoftwareController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var softwares = await _context.Softwares.ToListAsync();
            return Ok(softwares);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] List<Software> softwares)
        {
            if (softwares == null || softwares.Count == 0)
                return BadRequest("Lista vazia.");

            await _context.Softwares.AddRangeAsync(softwares);
            await _context.SaveChangesAsync();
            return Ok(softwares);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var software = await _context.Softwares.FindAsync(id);

            if (software == null)
                return NotFound("Não encontrado.");

            _context.Softwares.Remove(software);
            await _context.SaveChangesAsync();
            return Ok("Removido com sucesso.");
        }
    }
}
