using Microsoft.EntityFrameworkCore;
using sql_exercise_scoring.Data;
using sql_exercise_scoring.Models;

namespace sql_exercise_scoring.Repositories
{
    public class CoderRepository : ICoderRepository
    {
        private readonly ApplicationDbContext _context;

        public CoderRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Coder>> GetAllAsync()
        {
            return await _context.Coder.ToListAsync();
        }

        public async Task<Coder> GetByIdAsync(int id)
        {
            return await _context.Coder.FindAsync(id);
        }

        public async Task AddAsync(Coder coder)
        {
            await _context.Coder.AddAsync(coder);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Coder coder)
        {
            _context.Coder.Update(coder);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var coder = await _context.Coder.FindAsync(id);
            if (coder != null)
            {
                _context.Coder.Remove(coder);
                await _context.SaveChangesAsync();
            }
        }
    }
}
