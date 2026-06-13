using Bookstore.Api.Attributes;
using Bookstore.Shared.Dtos;
using Bookstore.Shared.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Bookstore.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoriesController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [HttpGet]
        public async Task<IActionResult> GetCategories()
        {
            var categories = await _categoryService.GetAllCategoriesAsync();
            return Ok(categories);
        }

        [HttpPost]
        [HasPermission("RESOUCRES.CREATE")]
        public async Task<ActionResult<ApiResponse>> Create([FromBody] CreateCategoryDto request)
        {
            if (!ModelState.IsValid) return BadRequest(new ApiResponse { Success = false, Message = "Dữ liệu không hợp lệ.", Data = ModelState });

            await _categoryService.CreateCategoryAsync(request);
            return Ok(new ApiResponse { Success = true, Message = "Thêm thể loại thành công." });
        }

        [HttpPut("{id}")]
        [HasPermission("RESOUCRES.UPDATE")]
        public async Task<ActionResult<ApiResponse>> Update(int id, [FromBody] UpdateCategoryDto request)
        {
            if (!ModelState.IsValid) return BadRequest(new ApiResponse { Success = false, Message = "Dữ liệu không hợp lệ.", Data = ModelState });

            var success = await _categoryService.UpdateCategoryAsync(id, request);
            if (!success) return NotFound(new ApiResponse { Success = false, Message = "Không tìm thấy thể loại để cập nhật." });

            return Ok(new ApiResponse { Success = true, Message = "Cập nhật thể loại thành công." });
        }

        [HttpDelete("{id}")]
        [HasPermission("RESOUCRES.DELETE")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _categoryService.DeleteCategoryAsync(id);
            if (!success) return NotFound(new { message = "Không tìm thấy thể loại để xóa." });

            return Ok(new { message = "Xóa thể loại thành công." });
        }
    }
}
