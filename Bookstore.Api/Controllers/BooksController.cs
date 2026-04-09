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

        [HttpGet("category")]
        public async Task<IActionResult> GetBooksByCID([FromQuery] int id)
        {
            var books = await _bookService.GetBooksByCategoryIdAsync(id);
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
