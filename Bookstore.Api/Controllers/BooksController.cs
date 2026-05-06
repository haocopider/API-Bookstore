using AutoMapper;
using Bookstore.Shared.Dtos;
using Bookstore.Shared.Interfaces;
using Bookstore.Shared.Models;
using Bookstore.Shared.Services;
using Microsoft.AspNetCore.Mvc;

namespace Bookstore.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BooksController : ControllerBase
    {
        private readonly IBookService _bookService;
        public BooksController(IBookService bookService)
        {
            _bookService = bookService;
        }

        [HttpGet]
        public async Task<IActionResult> GetBooks()
        {
            var result = await _bookService.GetAllBooksAsync();
            if (!result.Any()) return NotFound(new { Message = "Chưa có cuốn sách nào." });
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetBookById(int id)
        {
            var result = await _bookService.GetBookByIdAsync(id);
            if (result == null) return NotFound(new { Message = "Không tìm thấy sách." });
            return Ok(result);
        }

        [HttpPost("review")]
        public async Task<IActionResult> ReviewBook([FromBody] ReviewBookDto reviewRequest)
        {
            var success = await _bookService.ReviewBookAsync(reviewRequest);
            if (!success) return BadRequest(new { Message = "Đánh giá thất bại. Vui lòng thử lại." });
            return Ok(new { Message = "Đánh giá thành công." });
        }

        [HttpGet("categories")]
        public async Task<IActionResult> GetBooksByCID([FromQuery] int id)
        {
            var books = await _bookService.GetBooksByCategoryIdAsync(id);
            return Ok(books);
        }

        [HttpGet("promotions")]
        public async Task<IActionResult> GetBooksByPromotionId([FromQuery] int id)
        {
            var books = await _bookService.GetBooksByPromotionIdAsync(id);
            return Ok(books);
        }

        [HttpGet("authors")]
        public async Task<IActionResult> GetBooksByAuthorId([FromQuery] int id)
        {
            var books = await _bookService.GetBooksByAuthorIdAsync(id);
            return Ok(books);
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchBooks([FromQuery] BookFilterRequestDto filter)
        {
            var result = await _bookService.SearchBooksAsync(filter);
            if (!result.Any()) return NotFound(new { Message = "Không tìm thấy sách phù hợp." });
            return Ok(result);
        }

        [HttpGet("batch")]
        public async Task<IActionResult> GetBooksByIds([FromQuery] string ids)
        {
            if (string.IsNullOrEmpty(ids)) return Ok(new List<Book>());
            var idList = ids.Split(',').Select(int.Parse).ToList();

            var result = await _bookService.GetBooksByIdsAsync(idList);
            return Ok(result);
        }
    }
}
