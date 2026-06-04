using Bookstore.Shared.Dtos;
using Bookstore.Shared.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Bookstore.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthorsController : ControllerBase
    {
        private readonly IAuthorService _authorService;

        public AuthorsController(IAuthorService authorService)
        {
            _authorService = authorService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllAuthors()
        {
            var authors = await _authorService.GetAllAuthorsAsync();
            return Ok(authors);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAuthorById(int id)
        {
            var author = await _authorService.GetAuthorByIdAsync(id);
            if (author == null) return NotFound(new { Message = "Không tìm thấy tác giả." });
            return Ok(author);
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchAuthors([FromQuery] string name)
        {
            var authors = await _authorService.SearchAuthorsAsync(name);
            if (!authors.Any()) return NotFound(new { Message = "Không tìm thấy tác giả phù hợp." });
            return Ok(authors);
        }

        // [HasPermission("MANAGE_CATALOG")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateAuthorDto request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            await _authorService.CreateAuthorAsync(request);
            return Ok(new { message = "Thêm tác giả thành công." });
        }

        // [HasPermission("MANAGE_CATALOG")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateAuthorDto request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var success = await _authorService.UpdateAuthorAsync(id, request);
            if (!success) return NotFound(new { message = "Không tìm thấy tác giả để cập nhật." });

            return Ok(new { message = "Cập nhật tác giả thành công." });
        }

        // [HasPermission("MANAGE_CATALOG")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _authorService.DeleteAuthorAsync(id);
            if (!success) return NotFound(new { message = "Không tìm thấy tác giả để xóa." });

            return Ok(new { message = "Xóa tác giả thành công." });
        }
    }
}
