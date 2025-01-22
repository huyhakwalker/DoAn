using sql_exercise_scoring.Models;

namespace sql_exercise_scoring.Repositories
{
    public interface ICoderRepository
    {
        Task<IEnumerable<Coder>> GetAllAsync();
        Task<Coder> GetByIdAsync(int id);
        Task AddAsync(Coder coder);
        Task UpdateAsync(Coder coder);
        Task DeleteAsync(int id);
    }
}
