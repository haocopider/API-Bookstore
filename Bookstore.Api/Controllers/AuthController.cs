using Bookstore.Shared.Dtos;
using Bookstore.Shared.Interfaces;
using Bookstore.Shared.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace Bookstore.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpGet("me/profile")]
        public async Task<IActionResult> GetMyProfile()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");

                if (userIdClaim == null)
                {
                    return Unauthorized(new { message = "Token hợp lệ nhưng không chứa thông tin ID (NameIdentifier)." });
                }

                if (!int.TryParse(userIdClaim.Value, out int userId))
                {
                    return Unauthorized(new { message = $"Lỗi: ID trong Token không phải là số nguyên. Nó đang là: '{userIdClaim.Value}'" });
                }

                var profile = await _authService.GetProfileAsync(userId);

                if (profile == null)
                {
                    return NotFound(new { message = "Không tìm thấy người dùng này trong Database." });
                }

                return Ok(profile);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi máy chủ", error = ex.Message });
            }
        }

        [HttpPut("me/profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new { message = "Token không hợp lệ hoặc đã hết hạn." });
            }

            var success = await _authService.UpdateProfileAsync(userId, dto);

            if (!success)
            {
                return BadRequest(new { message = "Không thể cập nhật thông tin lúc này." });
            }

            return Ok(new { message = "Cập nhật thông tin thành công!" });
        }

        [HttpPut("me/change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new { message = "Token không hợp lệ hoặc đã hết hạn." });
            }

            var success = await _authService.ChangePasswordAsync(userId, dto);

            if (!success)
            {
                return BadRequest(new { message = "Mật khẩu cũ không chính xác." });
            }

            return Ok(new { message = "Đổi mật khẩu thành công!" });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _authService.RegisterAsync(request);

            if (result)
            {
                return Ok(new { message = "Đăng ký thành công!" });
            }

            return BadRequest(new { message = "Email này đã được sử dụng." });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var response = await _authService.LoginAsync(request);

                if (response == null)
                {
                    return Unauthorized(new { message = "Sai email hoặc mật khẩu." });
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("update-fcm-token")]
        public async Task<IActionResult> UpdateFcmToken([FromBody] UpdateFcmTokenRequest request)
        {
            // Lấy UserId của người dùng đang đăng nhập từ JWT Token Claims
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new { message = "Phiên đăng nhập không hợp lệ." });
            }
            
            var success = await _authService.UpdateFcmToken(userId, request.Token);

            if (!success)
            {
                return BadRequest(new { message = "Không thể cập nhật mã thiết bị." });
            }

            return Ok(new { message = "Cập nhật mã thiết bị thành công." });
        }
    }
}
