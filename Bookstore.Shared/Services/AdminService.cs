using AutoMapper;
using Bookstore.Shared.Dtos;
using Bookstore.Shared.Helpers;
using Bookstore.Shared.Interfaces;
using Bookstore.Shared.Models;

namespace Bookstore.Shared.Services
{
    public class AdminService : IAdminService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public AdminService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<AdminDto>> GetAllAdminsAsync()
        {
            var admins = await _unitOfWork.Admins.GetAllAsync(a => a.Role);
            return admins.Select(a => new AdminDto
            {
                Id = a.Id,
                Username = a.UserName,
                Email = a.Email,
                RoleId = a.RoleId,
                RoleName = a.Role?.Name,
                IsActive = a.IsActive
            });
        }

        public async Task<AdminDto?> GetAdminByIdAsync(int id)
        {
            var admins = await _unitOfWork.Admins.FindAsync(a => a.Id == id, a => a.Role);
            var admin = admins.FirstOrDefault();
            if (admin == null) return null;

            return new AdminDto
            {
                Id = admin.Id,
                Username = admin.UserName,
                Email = admin.Email,
                RoleId = admin.RoleId,
                RoleName = admin.Role?.Name,
                IsActive = admin.IsActive
            };
        }

        public async Task<bool> CreateAdminAsync(CreateAdminRequest request)
        {
            // Kiểm tra trùng lặp Username
            var existing = await _unitOfWork.Admins.FindAsync(a => a.UserName.ToLower() == request.Username.ToLower());
            if (existing.Any())
                throw new ArgumentException("Tên tài khoản nhân viên này đã tồn tại trong hệ thống.");

            // Mã hóa mật khẩu thông qua PasswordHelper của hệ thống
            string hashedPassword = PasswordHelper.Hash(request.Password);

            var admin = new Admin
            {
                UserName = request.Username,
                FirstName = request.Username,
                LastName = request.Username,
                Password = hashedPassword,
                Email = request.Email,
                RoleId = request.RoleId,
                IsActive = true
            };

            await _unitOfWork.Admins.AddAsync(admin);
            await _unitOfWork.CommitAsync();
            return true;
        }

        public async Task<bool> UpdateAdminAsync(int id, UpdateAdminRequest request)
        {
            var admins = await _unitOfWork.Admins.FindAsync(a => a.Id == id);
            var admin = admins.FirstOrDefault();
            if (admin == null) return false;

            admin.Email = request.Email;
            admin.RoleId = request.RoleId;
            admin.IsActive = request.IsActive;

            _unitOfWork.Admins.Update(admin);
            await _unitOfWork.CommitAsync();
            return true;
        }

        public async Task<IEnumerable<RoleDto>> GetRolesAsync()
        {
            // Lấy danh sách các vai trò kèm theo liên kết bảng trung gian RolePermissions và Permissions công khai
            var roles = await _unitOfWork.Roles.GetAllAsync(r => r.RolePermissions);

            // Do cần lấy thông tin tên quyền cụ thể, ta nạp tiếp dữ liệu từ Repo Permissions
            var allRolePermissions = await _unitOfWork.RolePermissions.GetAllAsync(rp => rp.Permission);

            return roles.Select(r => new RoleDto
            {
                Id = r.Id,
                Name = r.Name,
                Permissions = allRolePermissions
                    .Where(rp => rp.RoleId == r.Id && rp.Permission != null)
                    .Select(rp => rp.Permission!.Name)
                    .ToList()
            });
        }

        public async Task<bool> CreateRoleAsync(CreateRoleRequest request)
        {
            var existing = await _unitOfWork.Roles.FindAsync(r => r.Name.ToLower() == request.Name.ToLower());
            if (existing.Any())
                throw new ArgumentException("Tên vai trò này đã tồn tại.");

            var role = new Role { Name = request.Name };
            await _unitOfWork.Roles.AddAsync(role);
            await _unitOfWork.CommitAsync(); // Commit trước để sinh RoleId tự động

            if (request.PermissionIds != null && request.PermissionIds.Any())
            {
                foreach (var permId in request.PermissionIds)
                {
                    await _unitOfWork.RolePermissions.AddAsync(new RolePermission
                    {
                        RoleId = role.Id,
                        PermissionId = permId
                    });
                }
                await _unitOfWork.CommitAsync();
            }

            return true;
        }

        public async Task<bool> UpdateRolePermissionsAsync(int roleId, List<int> permissionIds)
        {
            var roles = await _unitOfWork.Roles.FindAsync(r => r.Id == roleId);
            if (!roles.Any()) return false;

            // Xóa toàn bộ cấu hình quyền cũ của vai trò này trong bảng nối dữ liệu trung gian
            var oldMappings = await _unitOfWork.RolePermissions.FindAsync(rp => rp.RoleId == roleId);
            foreach (var mapping in oldMappings)
            {
                _unitOfWork.RolePermissions.Remove(mapping);
            }
            await _unitOfWork.CommitAsync();

            // Thiết lập đồng bộ các quyền mới được bàn giao từ Request
            if (permissionIds != null && permissionIds.Any())
            {
                foreach (var permId in permissionIds)
                {
                    await _unitOfWork.RolePermissions.AddAsync(new RolePermission
                    {
                        RoleId = roleId,
                        PermissionId = permId
                    });
                }
                await _unitOfWork.CommitAsync();
            }

            return true;
        }

        public async Task<IEnumerable<PermissionDto>> GetAllPermissionsAsync()
        {
            var permissions = await _unitOfWork.Permissions.GetAllAsync();
            return permissions.Select(p => new PermissionDto
            {
                Id = p.Id,
                Name = p.Name,
                ActionName = p.Name
            });
        }
    }
}