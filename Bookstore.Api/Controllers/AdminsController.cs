using Bookstore.Api.Attributes;
using Bookstore.Shared.Dtos;
using Bookstore.Shared.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Bookstore.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminsController : ControllerBase
    {
        private readonly IAdminService _adminService;

        public AdminsController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginAdminRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var response = await _adminService.LoginAdminAsync(request);
            if (!response.IsSuccess) return Unauthorized(new { message = response.ErrorMessage });
            return Ok(new { token = response.Token, refreshToken = response.RefreshToken });
        }

        [HttpGet("staff")]
        [HasPermission("STAFF.VIEW")]
        public async Task<IActionResult> GetAllStaff()
        {
            var staffList = await _adminService.GetAllAdminsAsync();
            return Ok(staffList);
        }

        [HttpGet("staff/{id}")]
        [HasPermission("STAFF.VIEW")]
        public async Task<IActionResult> GetStaffById(int id)
        {
            var staff = await _adminService.GetAdminByIdAsync(id);
            if (staff == null) return NotFound(new { message = "Không tìm thấy nhân viên." });
            return Ok(staff);
        }

        [HttpPost("staff")]
        [HasPermission("STAFF.CREATE")]
        public async Task<ActionResult<ApiResponse>> CreateStaff([FromBody] CreateAdminRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(new ApiResponse { Success = false, Message = "Dữ liệu không hợp lệ.", Data = ModelState });

            try
            {
                await _adminService.CreateAdminAsync(request);
                return Ok(new ApiResponse { Success = true, Message = "Thêm mới nhân viên thành công!" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiResponse { Success = false, Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse { Success = false, Message = "Lỗi hệ thống", Data = ex.Message });
            }
        }

        [HttpPut("staff/{id}")]
        [HasPermission("STAFF.UPDATE")]
        public async Task<ActionResult<ApiResponse>> UpdateStaff(int id, [FromBody] UpdateAdminRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(new ApiResponse { Success = false, Message = "Dữ liệu không hợp lệ.", Data = ModelState });

            var success = await _adminService.UpdateAdminAsync(id, request);
            if (!success) return NotFound(new ApiResponse { Success = false, Message = "Không tìm thấy tài khoản nhân viên cần sửa." });

            return Ok(new ApiResponse { Success = true, Message = "Cập nhật tài khoản nhân viên thành công!" });
        }

        // --- ENDPOINTS QUẢN LÝ VAI TRÒ & PHÂN QUYỀN (ROLE/PERMISSION) ---

        [HttpGet("roles")]
        [HasPermission("ROLE.VIEW")]
        public async Task<IActionResult> GetRoles()
        {
            var roles = await _adminService.GetRolesAsync();
            return Ok(roles);
        }

        [HttpPost("roles")]
        [HasPermission("ROLE.CREATE")]
        public async Task<ActionResult<ApiResponse>> CreateRole([FromBody] CreateRoleRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(new ApiResponse { Success = false, Message = "Dữ liệu không hợp lệ.", Data = ModelState });

            try
            {
                await _adminService.CreateRoleAsync(request);
                return Ok(new ApiResponse { Success = true, Message = "Tạo vai trò thành công!" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiResponse { Success = false, Message = ex.Message });
            }
        }

        [HttpPut("roles/{id}/permissions")]
        [HasPermission("ROLE.UPDATE")]
        public async Task<ActionResult<ApiResponse>> AssignPermissions(int id, [FromBody] List<int> permissionIds)
        {
            var success = await _adminService.UpdateRolePermissionsAsync(id, permissionIds);
            if (!success) return NotFound(new ApiResponse { Success = false, Message = "Không tìm thấy vai trò cấu hình." });

            return Ok(new ApiResponse { Success = true, Message = "Cập nhật và cấp quyền cho vai trò thành công!" });
        }

        [HttpGet("permissions")]
        [HasPermission("PERM.VIEW")]
        public async Task<IActionResult> GetAllPermissions()
        {
            var permissions = await _adminService.GetAllPermissionsAsync();
            return Ok(permissions);
        }
    }
}