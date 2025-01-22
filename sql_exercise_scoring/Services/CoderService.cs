using sql_exercise_scoring.DTOs;
using sql_exercise_scoring.Models;
using sql_exercise_scoring.Repositories;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace sql_exercise_scoring.Services
{
    public class CoderService : ICoderService
    {
        private readonly ICoderRepository _coderRepository;

        public CoderService(ICoderRepository coderRepository)
        {
            _coderRepository = coderRepository;
        }

        public async Task<IEnumerable<CoderDTO>> GetAllCodersAsync()
        {
            var coders = await _coderRepository.GetAllAsync();
            return coders.Select(c => new CoderDTO
            {
                CoderId = c.CoderId,
                CoderName = c.CoderName,
                CoderEmail = c.CoderEmail,
                PwdMd5Coder = c.PwdMd5,
                AdminCoder = c.AdminCoder,
                ContestSetter = c.ContestSetter,
                RegisterDate = c.RegisterDate
            });
        }

        public async Task<CoderDTO> GetCoderByIdAsync(int id)
        {
            var coder = await _coderRepository.GetByIdAsync(id);
            if (coder == null) return null;

            return new CoderDTO
            {
                CoderId = coder.CoderId,
                CoderName = coder.CoderName,
                CoderEmail = coder.CoderEmail,
                PwdMd5Coder = coder.PwdMd5,
                AdminCoder = coder.AdminCoder,
                ContestSetter = coder.ContestSetter,
                RegisterDate = coder.RegisterDate
            };
        }

        public async Task AddCoderAsync(CoderDTO coderDto)
        {
            var coder = new Coder
            {
                CoderName = coderDto.CoderName,
                CoderEmail = coderDto.CoderEmail,
                PwdMd5 = coderDto.PwdMd5Coder,
                AdminCoder = coderDto.AdminCoder,
                ContestSetter = coderDto.ContestSetter,
                RegisterDate = coderDto.RegisterDate
            };

            await _coderRepository.AddAsync(coder);
        }

        public async Task UpdateCoderAsync(int id, CoderDTO coderDto)
        {
            var existingCoder = await _coderRepository.GetByIdAsync(id);
            if (existingCoder == null)
            {
                throw new KeyNotFoundException("Coder not found");
            }

            existingCoder.CoderName = coderDto.CoderName;
            existingCoder.CoderEmail = coderDto.CoderEmail;
            existingCoder.PwdMd5 = coderDto.PwdMd5Coder;
            existingCoder.AdminCoder = coderDto.AdminCoder;
            existingCoder.ContestSetter = coderDto.ContestSetter;
            existingCoder.RegisterDate = coderDto.RegisterDate;

            await _coderRepository.UpdateAsync(existingCoder);
        }

        public async Task DeleteCoderAsync(int id)
        {
            var existingCoder = await _coderRepository.GetByIdAsync(id);
            if (existingCoder == null)
            {
                throw new KeyNotFoundException("Coder not found");
            }

            await _coderRepository.DeleteAsync(id);
        }
    }
}
