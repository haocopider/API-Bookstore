using Bookstore.Shared.Dtos;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bookstore.Shared.Interfaces
{
    public interface IAdminService
    {
        // Quản lý tài khoản Admin/Nhân viên
        Task<IEnumerable<AdminDto>> GetAllAdminsAsync();
        Task<AdminDto?> GetAdminByIdAsync(int id);
        Task<bool> CreateAdminAsync(CreateAdminRequest request);
        Task<bool> UpdateAdminAsync(int id, UpdateAdminRequest request);

        // Quản lý Vai trò (Role) & Cấp quyền
        Task<IEnumerable<RoleDto>> GetRolesAsync();
        Task<bool> CreateRoleAsync(CreateRoleRequest request);
        Task<bool> UpdateRolePermissionsAsync(int roleId, List<int> permissionIds);

        // Lấy danh sách tất cả các Quyền hiện có trong hệ thống
        Task<IEnumerable<PermissionDto>> GetAllPermissionsAsync();
    }
}
