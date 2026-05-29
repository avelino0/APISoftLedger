using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SoftLedger.Data;
using System.Reflection;
using System.Text.Json;

namespace SoftLedger.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class GenericController : ControllerBase
    {
        private readonly AppDbContext _context;

        public GenericController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Get([FromQuery] string table, [FromQuery] string? field, [FromQuery] string? value)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(table))
                    return BadRequest(new { error = "Tabela não informada." });

                var dbSetProperty = _context.GetType().GetProperty(table,
                    BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

                if (dbSetProperty == null)
                    return NotFound(new { error = $"Tabela '{table}' não encontrada." });

                var dbSet = dbSetProperty.GetValue(_context);

                if (dbSet == null)
                    return StatusCode(500, new { error = "Erro ao acessar tabela." });

                var data = ((IEnumerable<object>)dbSet).ToList();

                if (!data.Any())
                    return Ok(new List<object>());

                if (string.IsNullOrWhiteSpace(field))
                    return Ok(data);

                var firstItem = data.First();

                var property = firstItem.GetType().GetProperty(
                    field,
                    BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

                if (property == null)
                    return NotFound(new { error = $"Campo '{field}' não encontrado." });

                var result = data.Where(x =>
                {
                    var prop = x.GetType().GetProperty(field,
                        BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                    var propValue = prop?.GetValue(x)?.ToString();
                    return propValue != null &&
                           propValue.Contains(value ?? string.Empty, StringComparison.OrdinalIgnoreCase);
                });

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Erro interno.", details = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromQuery] string table, [FromBody] JsonElement body)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(table))
                    return BadRequest(new { error = "Tabela não informada." });

                var dbSetProperty = _context.GetType().GetProperty(table,
                    BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

                if (dbSetProperty == null)
                    return NotFound(new { error = $"Tabela '{table}' não encontrada." });

                var entityType = dbSetProperty.PropertyType.GetGenericArguments()[0];
                var listType = typeof(List<>).MakeGenericType(entityType);

                var items = JsonSerializer.Deserialize(body.GetRawText(), listType,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (items == null)
                    return BadRequest(new { error = "Corpo da requisição inválido." });

                await _context.AddRangeAsync((IEnumerable<object>)items);
                await _context.SaveChangesAsync();
                return Ok(items);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Erro interno.", details = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete([FromQuery] string table, Guid id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(table))
                    return BadRequest(new { error = "Tabela não informada." });

                var dbSetProperty = _context.GetType().GetProperty(table,
                    BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

                if (dbSetProperty == null)
                    return NotFound(new { error = $"Tabela '{table}' não encontrada." });

                var entityType = dbSetProperty.PropertyType.GetGenericArguments()[0];
                var entity = await _context.FindAsync(entityType, new object[] { id });

                if (entity == null)
                    return NotFound(new { error = "Não encontrado." });

                _context.Remove(entity);
                await _context.SaveChangesAsync();
                return Ok("Removido com sucesso.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Erro interno.", details = ex.Message });
            }
        }
    }
}
