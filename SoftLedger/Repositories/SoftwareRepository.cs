using Microsoft.EntityFrameworkCore;
using SoftLedger.Data;
using SoftLedger.Models;

namespace SoftLedger.Repositories
{
    public class SoftwareRepository
    {
        private readonly AppDbContext _context;

        public SoftwareRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Software>> GetAllAsync()
            => await _context.Softwares.ToListAsync();

        public async Task AddRangeAsync(IEnumerable<Software> softwares)
        {
            await _context.Softwares.AddRangeAsync(softwares);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var software = await _context.Softwares.FindAsync(id);
            if (software == null) return false;

            _context.Softwares.Remove(software);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}

