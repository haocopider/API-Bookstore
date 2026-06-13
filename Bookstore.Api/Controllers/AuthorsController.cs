using Bookstore.Api.Attributes;
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

        [HttpPost]
        [HasPermission("RESOUCRES.CREATE")]
        public async Task<ActionResult<ApiResponse>> Create([FromBody] CreateAuthorDto request)
        {
            if (!ModelState.IsValid) return BadRequest(new ApiResponse { Success = false, Message = "Dữ liệu không hợp lệ.", Data = ModelState });

            await _authorService.CreateAuthorAsync(request);
            return Ok(new ApiResponse { Success = true, Message = "Thêm tác giả thành công." });
        }

        [HttpPut("{id}")]
        [HasPermission("RESOUCRES.UPDATE")]
        public async Task<ActionResult<ApiResponse>> Update(int id, [FromBody] UpdateAuthorDto request)
        {
            if (!ModelState.IsValid) return BadRequest(new ApiResponse { Success = false, Message = "Dữ liệu không hợp lệ.", Data = ModelState });

            var success = await _authorService.UpdateAuthorAsync(id, request);
            if (!success) return NotFound(new ApiResponse { Success = false, Message = "Không tìm thấy tác giả để cập nhật." });

            return Ok(new ApiResponse { Success = true, Message = "Cập nhật tác giả thành công." });
        }

        [HttpDelete("{id}")]
        [HasPermission("RESOUCRES.DELETE")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _authorService.DeleteAuthorAsync(id);
            if (!success) return NotFound(new { message = "Không tìm thấy tác giả để xóa." });

            return Ok(new { message = "Xóa tác giả thành công." });
        }
    }
}
