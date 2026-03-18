using Bookstore.Api.Services;
using Bookstore.Shared.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Bookstore.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BooksController : ControllerBase
    {
        private readonly BookService _bookservice;

        public BooksController(BookService bookservice)
        {
            _bookservice = bookservice;
        }

        [HttpGet]
        public async Task<ActionResult<List<BookDto>>> Get()
        {
            var books = await _bookservice.GetAllBooksAsync();
            return Ok(books);
        }
    }
}
