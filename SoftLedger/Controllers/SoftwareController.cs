using Microsoft.AspNetCore.Mvc;
using SoftLedger.Models;
using Microsoft.AspNetCore.Authorization;

namespace SoftLedger.Controllers
{


    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class SoftwareController : ControllerBase
    {
        private static List<Software> _softwares = new();

        [HttpGet]
        public IActionResult Get([FromQuery] string? name, [FromQuery] Guid? id)
        {
            var query = _softwares.AsQueryable();

            if (id.HasValue)
                query = query.Where(x => x.Id == id.Value);

            if (!string.IsNullOrEmpty(name))
                query = query.Where(x => x.SoftwareName != null && x.SoftwareName.Contains(name));

            return Ok(query.ToList());
        }

        [HttpPost]
        public IActionResult Create([FromBody] List<Software> softwares)
        {
            if (softwares == null || softwares.Count == 0)
                return BadRequest("Lista vazia.");

            _softwares.AddRange(softwares);
            return Ok(_softwares);
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(Guid id)
        {
            var software = _softwares.FirstOrDefault(x => x.Id == id);

            if (software == null)
                return NotFound("Não encontrado.");

            _softwares.Remove(software);
            return Ok("Removido com sucesso.");
        }
    }
}
