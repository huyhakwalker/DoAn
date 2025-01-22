using Microsoft.AspNetCore.Mvc;
using sql_exercise_scoring.DTOs;
using sql_exercise_scoring.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace YourNamespace.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CoderController : ControllerBase
    {
        private readonly ICoderService _coderService;

        public CoderController(ICoderService coderService)
        {
            _coderService = coderService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllCoders()
        {
            var coders = await _coderService.GetAllCodersAsync();
            return Ok(coders);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCoderById(int id)
        {
            var coder = await _coderService.GetCoderByIdAsync(id);
            if (coder == null)
            {
                return NotFound();
            }
            return Ok(coder);
        }

        [HttpPost]
        public async Task<IActionResult> AddCoder([FromBody] CoderDTO coderDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await _coderService.AddCoderAsync(coderDto);
            return CreatedAtAction(nameof(GetCoderById), new { id = coderDto.CoderId }, coderDto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCoder(int id, [FromBody] CoderDTO coderDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existingCoder = await _coderService.GetCoderByIdAsync(id);
            if (existingCoder == null)
            {
                return NotFound();
            }

            await _coderService.UpdateCoderAsync(id, coderDto);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCoder(int id)
        {
            var existingCoder = await _coderService.GetCoderByIdAsync(id);
            if (existingCoder == null)
            {
                return NotFound();
            }

            await _coderService.DeleteCoderAsync(id);
            return NoContent();
        }
    }
}
