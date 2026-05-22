using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SoftLedger.Data;
using SoftLedger.Models;
using System.Reflection;

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
        public IActionResult Get([FromQuery] string table, [FromQuery] string field, [FromQuery] string value)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(table))
                    return BadRequest(new
                    {
                        error = "Tabela não informada."
                    });

                if (string.IsNullOrWhiteSpace(field))
                    return BadRequest(new
                    {
                        error = "Campo não informado."
                    });

                // Procura a tabela no DbContext
                var dbSetProperty = _context.GetType().GetProperty(table,
                    System.Reflection.BindingFlags.IgnoreCase |
                    System.Reflection.BindingFlags.Public |
                    System.Reflection.BindingFlags.Instance
                );

                if (dbSetProperty == null)
                    return NotFound(new
                    {
                        error = $"Tabela '{table}' não encontrada."
                    });

                var dbSet = dbSetProperty.GetValue(_context);

                if (dbSet == null)
                    return StatusCode(500, new
                    {
                        error = "Erro ao acessar tabela."
                    });

                var data = ((IEnumerable<object>)dbSet).ToList();

                if (!data.Any())
                    return Ok(new List<object>());

                var firstItem = data.First();

                var property = firstItem.GetType().GetProperty(
                    field,
                    System.Reflection.BindingFlags.IgnoreCase |
                    System.Reflection.BindingFlags.Public |
                    System.Reflection.BindingFlags.Instance
                );

                if (property == null)
                    return NotFound(new
                    {
                        error = $"Campo '{field}' não encontrado."
                    });

                var result = data.Where(x =>
                {
                    var prop = x.GetType().GetProperty(field,
                        System.Reflection.BindingFlags.IgnoreCase |
                        System.Reflection.BindingFlags.Public |
                        System.Reflection.BindingFlags.Instance
                    );

                    var propValue = prop?.GetValue(x)?.ToString();

                    return propValue != null &&
                           propValue.Contains(
                               value,
                               StringComparison.OrdinalIgnoreCase
                           );
                });

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    error = "Erro interno.",
                    details = ex.Message
                });
            }
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
