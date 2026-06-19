using AutoMapper;
using Bookstore.Api.Attributes;
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

        [HttpDelete("admin/delete/{id}")]
        [HasPermission("RESOUCRES.DELETE")]
        public async Task<IActionResult> DeleteBook(int id)
        {
            try
            {
                var success = await _bookService.DeleteBookAsync(id);
                if (success) return Ok(new ApiResponse { Success = true, Message = "Xóa sách thành công." });

                return NotFound(new ApiResponse { Success = false, Message = "Không tìm thấy sách hoặc đã xóa." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse { Success = false, Message = "Lỗi hệ thống: " + ex.Message });
            }
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

        [HttpPost("review")]
        public async Task<ActionResult<ApiResponse>> ReviewBook([FromBody] ReviewBookDto reviewRequest)
        {
            var success = await _bookService.ReviewBookAsync(reviewRequest);
            if (!success) return BadRequest(new ApiResponse { Success = false, Message = "Đánh giá thất bại. Vui lòng thử lại." });
            return Ok(new ApiResponse { Success = true, Message = "Đánh giá thành công." });
        }

        [HttpGet("admin")]
        [HasPermission("RESOUCRES.VIEW")]
        public async Task<IActionResult> GetAllBookForAdmin([FromQuery] BookFilterRequestDto filter)
        {
            var books = await _bookService.GetAllBooksForAdminAsync(filter);
            return Ok(books);
        }

        [HttpGet("admin/{id}")]
        [HasPermission("RESOUCRES.VIEW")]
        public async Task<IActionResult> GetBookForAdmin(int id)
        {
            var book = await _bookService.GetBookAdminByIdAsync(id);
            if (book == null) return NotFound(new { Message = "Không tìm thấy sách." });
            return Ok(book);
        }

        [HttpPost("admin/create")]
        [HasPermission("RESOUCRES.CREATE")]
        public async Task<ActionResult<ApiResponse>> CreateBook([FromBody] CreateBookDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse { Success = false, Message = "Dữ liệu không hợp lệ.", Data = ModelState });

            try
            {
                var success = await _bookService.CreateBookAsync(request);
                if (success)
                {
                    return Ok(new ApiResponse { Success = true, Message = "Tạo sách thành công!" });
                }
                return BadRequest(new ApiResponse { Success = false, Message = "Có lỗi xảy ra khi lưu sách." });
            }
            catch (Exception ex)
            {
                // Có thể log lỗi ở đây
                return StatusCode(500, new ApiResponse { Success = false, Message = "Lỗi hệ thống: " + ex.Message });
            }
        }

        [HttpPut("admin/update/{id}")]
        [HasPermission("RESOUCRES.UPDATE")]
        public async Task<IActionResult> UpdateBook(int id, [FromBody] UpdateBookDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var success = await _bookService.UpdateBookAsync(id, request);
                if (success) return Ok(new { Message = "Cập nhật sách thành công!" });

                return NotFound(new { Message = "Không tìm thấy sách hoặc sách đã bị xóa." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Lỗi hệ thống: " + ex.Message });
            }
        }

        [HttpPost("admin/restock")]
        [HasPermission("RESOUCRES.UPDATE")]
        public async Task<ActionResult<ApiResponse>> Restock([FromBody] RestockDto request)
        {
            if (request.AddedQuantity <= 0)
                return BadRequest(new ApiResponse { Success = false, Message = "Số lượng nhập kho phải lớn hơn 0." });

            try
            {
                var success = await _bookService.RestockAsync(request);
                if (success) return Ok(new ApiResponse { Success = true, Message = "Nhập kho thành công!" });

                return NotFound(new ApiResponse { Success = false, Message = "Không tìm thấy định dạng sách này." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse { Success = false, Message = "Lỗi hệ thống: " + ex.Message });
            }
        }
    }
}
