using sql_exercise_scoring.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace sql_exercise_scoring.Services
{
    public interface ICoderService
    {
        Task<IEnumerable<CoderDTO>> GetAllCodersAsync();
        Task<CoderDTO> GetCoderByIdAsync(int id);
        Task AddCoderAsync(CoderDTO coderDTO);
        Task UpdateCoderAsync(int id, CoderDTO coderDTO);
        Task DeleteCoderAsync(int id);
    }
}
