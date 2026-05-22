using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Bookstore.Shared.Dtos
{
    public class AdminDto
    {
        public int Id { get; set; }
        public string Username { get; set; } = null!;
        public string? Email { get; set; }
        public int RoleId { get; set; }
        public string? RoleName { get; set; }
        public bool IsActive { get; set; }
    }

    public class CreateAdminRequest
    {
        [Required]
        public string Username { get; set; } = null!;

        [Required]
        [MinLength(6, ErrorMessage = "Mật khẩu phải từ 6 ký tự trở lên")]
        public string Password { get; set; } = null!;

        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        public string? Email { get; set; }

        [Required]
        public int RoleId { get; set; }
    }

    public class UpdateAdminRequest
    {
        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        public string? Email { get; set; }

        [Required]
        public int RoleId { get; set; }
        public bool IsActive { get; set; }
    }

    // --- DTOs cho Vai trò & Quyền ---
    public class RoleDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public List<string> Permissions { get; set; } = new List<string>();
    }

    public class CreateRoleRequest
    {
        [Required]
        public string Name { get; set; } = null!;
        public List<int> PermissionIds { get; set; } = new List<int>();
    }

    public class PermissionDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string ActionName { get; set; } = null!;
    }
}
