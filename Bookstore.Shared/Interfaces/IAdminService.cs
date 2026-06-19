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
        Task<AdminResponseDto> LoginAdminAsync(LoginAdminRequest request);

        // Quản lý Vai trò (Role) & Cấp quyền
        Task<IEnumerable<RoleWithPermissionsDto>> GetRolesAsync();
        Task<bool> CreateRoleAsync(CreateRoleRequest request);
        Task<bool> UpdateRolePermissionsAsync(int id, List<int> permissionIds);
        Task<IEnumerable<PermissionDto>> GetAllPermissionsAsync();

        //Quản lý hành động 
        Task<List<AuditLogDto>> GetRecentLogsAsync(int take = 50);
        Task<bool> UndoActionAsync(int auditLogId, int currentAdminId);
    }
}
