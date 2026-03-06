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
        public IActionResult GetAll()
        {
            return Ok(_softwares);
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
