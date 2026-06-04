using Bookstore.Api.Attributes;
using Bookstore.Shared.Dtos;
using Bookstore.Shared.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Bookstore.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[HasPermission("MANAGE_HR")]
    public class AdminsController : ControllerBase
    {
        private readonly IAdminService _adminService;

        public AdminsController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        // --- ENDPOINTS QUẢN LÝ NHÂN VIÊN (ADMIN) ---
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginAdminRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var response = await _adminService.LoginAdminAsync(request);
            if (!response.IsSuccess) return Unauthorized(new { message = response.ErrorMessage });
            return Ok(new { token = response.Token, refreshToken = response.RefreshToken });
        }

        [HttpGet("staff")]
        public async Task<IActionResult> GetAllStaff()
        {
            var staffList = await _adminService.GetAllAdminsAsync();
            return Ok(staffList);
        }

        [HttpGet("staff/{id}")]
        public async Task<IActionResult> GetStaffById(int id)
        {
            var staff = await _adminService.GetAdminByIdAsync(id);
            if (staff == null) return NotFound(new { message = "Không tìm thấy nhân viên." });
            return Ok(staff);
        }

        [HttpPost("staff")]
        public async Task<IActionResult> CreateStaff([FromBody] CreateAdminRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                await _adminService.CreateAdminAsync(request);
                return Ok(new { message = "Thêm mới nhân viên thành công!" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi hệ thống", error = ex.Message });
            }
        }

        [HttpPut("staff/{id}")]
        public async Task<IActionResult> UpdateStaff(int id, [FromBody] UpdateAdminRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var success = await _adminService.UpdateAdminAsync(id, request);
            if (!success) return NotFound(new { message = "Không tìm thấy tài khoản nhân viên cần sửa." });

            return Ok(new { message = "Cập nhật tài khoản nhân viên thành công!" });
        }

        // --- ENDPOINTS QUẢN LÝ VAI TRÒ & PHÂN QUYỀN (ROLE/PERMISSION) ---

        [HttpGet("roles")]
        public async Task<IActionResult> GetRoles()
        {
            var roles = await _adminService.GetRolesAsync();
            return Ok(roles);
        }

        [HttpPost("roles")]
        public async Task<IActionResult> CreateRole([FromBody] CreateRoleRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                await _adminService.CreateRoleAsync(request);
                return Ok(new { message = "Tạo vai trò thành công!" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("roles/{id}/permissions")]
        public async Task<IActionResult> AssignPermissions(int id, [FromBody] List<int> permissionIds)
        {
            var success = await _adminService.UpdateRolePermissionsAsync(id, permissionIds);
            if (!success) return NotFound(new { message = "Không tìm thấy vai trò cấu hình." });

            return Ok(new { message = "Cập nhật và cấp quyền cho vai trò thành công!" });
        }

        [HttpGet("permissions")]
        public async Task<IActionResult> GetAllPermissions()
        {
            var permissions = await _adminService.GetAllPermissionsAsync();
            return Ok(permissions);
        }
    }
}