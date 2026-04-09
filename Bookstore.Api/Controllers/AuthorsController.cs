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
    }
}
