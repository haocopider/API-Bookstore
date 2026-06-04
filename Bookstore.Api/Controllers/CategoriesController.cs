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

        // [HasPermission("MANAGE_CATALOG")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCategoryDto request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            await _categoryService.CreateCategoryAsync(request);
            return Ok(new { message = "Thêm thể loại thành công." });
        }

        // [HasPermission("MANAGE_CATALOG")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateCategoryDto request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var success = await _categoryService.UpdateCategoryAsync(id, request);
            if (!success) return NotFound(new { message = "Không tìm thấy thể loại để cập nhật." });

            return Ok(new { message = "Cập nhật thể loại thành công." });
        }

        // [HasPermission("MANAGE_CATALOG")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _categoryService.DeleteCategoryAsync(id);
            if (!success) return NotFound(new { message = "Không tìm thấy thể loại để xóa." });

            return Ok(new { message = "Xóa thể loại thành công." });
        }
    }
}
