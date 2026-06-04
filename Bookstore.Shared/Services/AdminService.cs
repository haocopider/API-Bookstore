using AutoMapper;
using Bookstore.Shared.Dtos;
using Bookstore.Shared.Helpers;
using Bookstore.Shared.Interfaces;
using Bookstore.Shared.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Bookstore.Shared.Services
{
    public class AdminService : IAdminService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;

        public AdminService(IUnitOfWork unitOfWork, IMapper mapper, IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _configuration = configuration;
        }

        public async Task<AdminResponseDto> LoginAdminAsync(LoginAdminRequest request)
        {
            var admin = await _unitOfWork.Admins.GetFirstOrDefaultAsync(a => a.UserName.ToLower() == request.UserName.ToLower());
            if (admin == null) return new AdminResponseDto { IsSuccess = false, ErrorMessage = "Tên đăng nhập hoặc mật khẩu không đúng." };

            // Kiểm tra mật khẩu (giả định có phương thức kiểm tra mật khẩu)
            if (!PasswordHelper.Verify(request.Password, admin.Password))
                return new AdminResponseDto { IsSuccess = false, ErrorMessage = "Tên đăng nhập hoặc mật khẩu không đúng." };

            //Kiểm tra role và lấy ra các permission của role 
            var token = GenerateJwtToken(admin);
            var refreshToken = GenerateRefreshToken();

            return new AdminResponseDto { IsSuccess = true, Token = token, RefreshToken = refreshToken };
        }

        private string GenerateJwtToken(Admin admin)
        {
            var role = _unitOfWork.Roles.GetFirstOrDefaultAsync(r => r.Id == admin.RoleId).Result;

            var rolepermissions = _unitOfWork.RolePermissions.FindAsync(rp => rp.RoleId == role.Id)
                .Result
                .Select(rp => rp.PermissionId)
                .ToList();
            var permissions = _unitOfWork.Permissions.FindAsync(p => rolepermissions.Contains(p.Id))
                .Result
                .Select(p => p.Code)
                .ToList();

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, admin.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, admin.UserName ?? ""),
                new Claim(ClaimTypes.Name, admin.FirstName + " " + admin.LastName ?? ""),
                new Claim("role", role.Code),
                new Claim("permissions", string.Join(",", permissions)),
            };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!)
            );

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(48),
                SigningCredentials = creds,
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"]
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
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

        public async Task<IEnumerable<RoleWithPermissionsDto>> GetRolesAsync()
        {
            var roles = await _unitOfWork.Roles.GetAllAsync();
            var allRolePermissions = await _unitOfWork.RolePermissions.GetAllAsync(rp => rp.Permission);

            return roles.Select(r =>
            {
                var rolePerms = allRolePermissions.Where(rp => rp.RoleId == r.Id && rp.Permission != null).ToList();

                return new RoleWithPermissionsDto
                {
                    Id = r.Id,
                    Code = r.Code,
                    Name = r.Name,
                    Description = r.Description,
                    IsSystem = r.IsSystem,
                    CreatedAt = r.CreatedAt,
                    AssignedPermissionIds = rolePerms.Select(rp => rp.PermissionId).ToList(),
                    Permissions = rolePerms.Select(rp => new PermissionDto
                    {
                        Id = rp.Permission!.Id,
                        Code = rp.Permission.Code,
                        Name = rp.Permission.Name,
                        Description = rp.Permission.Description,
                        CreatedAt = rp.Permission.CreatedAt
                    }).ToList()
                };
            });
        }

        public async Task<bool> CreateRoleAsync(CreateRoleRequest request)
        {
            // Tối ưu: Kiểm tra trùng lặp cả Code lẫn Name
            var existing = await _unitOfWork.Roles.FindAsync(r =>
                r.Code.ToLower() == request.Code.ToLower() ||
                r.Name.ToLower() == request.Name.ToLower());

            if (existing.Any())
                throw new ArgumentException("Mã (Code) hoặc Tên (Name) vai trò này đã tồn tại.");

            // Mapping đầy đủ các trường tương đồng với Model
            var role = new Role
            {
                Code = request.Code,
                Name = request.Name,
                Description = request.Description,
                IsSystem = false, // Mặc định role do người dùng tạo không phải là hệ thống
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Roles.AddAsync(role);
            await _unitOfWork.CommitAsync(); // Lưu để EF Core sinh ra role.Id

            if (request.PermissionIds != null && request.PermissionIds.Any())
            {
                // Tối ưu: Dùng Distinct() để loại bỏ các ID quyền bị client gửi trùng lặp, tránh lỗi DB
                var uniquePermissionIds = request.PermissionIds.Distinct().ToList();
                foreach (var permId in uniquePermissionIds)
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

        public async Task<bool> UpdateRoleAsync(int id, UpdateRoleRequest request)
        {
            var roles = await _unitOfWork.Roles.FindAsync(r => r.Id == id);
            var role = roles.FirstOrDefault();

            if (role == null) return false;
            if (role.IsSystem)
                throw new ArgumentException("Không được phép sửa đổi thông tin của vai trò hệ thống.");

            var existing = await _unitOfWork.Roles.FindAsync(r =>
                r.Id != id &&
                (r.Code.ToLower() == request.Code.ToLower() || r.Name.ToLower() == request.Name.ToLower()));

            if (existing.Any())
                throw new ArgumentException("Mã (Code) hoặc Tên (Name) vai trò này đã bị sử dụng bởi một vai trò khác.");

            role.Code = request.Code;
            role.Name = request.Name;
            role.Description = request.Description;

            _unitOfWork.Roles.Update(role);
            await _unitOfWork.CommitAsync();
            return true;
        }

        public async Task<bool> UpdateRolePermissionsAsync(int roleId, List<int> permissionIds)
        {
            var roles = await _unitOfWork.Roles.FindAsync(r => r.Id == roleId);
            var role = roles.FirstOrDefault();

            if (role == null) return false;

            if (role.IsSystem)
                throw new ArgumentException("Không được phép thay đổi phân quyền của vai trò hệ thống.");

            var oldMappings = await _unitOfWork.RolePermissions.FindAsync(rp => rp.RoleId == roleId);
            foreach (var mapping in oldMappings)
            {
                _unitOfWork.RolePermissions.Remove(mapping);
            }

            if (permissionIds != null && permissionIds.Any())
            {
                var uniquePermissionIds = permissionIds.Distinct().ToList();
                foreach (var permId in uniquePermissionIds)
                {
                    await _unitOfWork.RolePermissions.AddAsync(new RolePermission
                    {
                        RoleId = roleId,
                        PermissionId = permId
                    });
                }
            }

            await _unitOfWork.CommitAsync();

            return true;
        }

        public async Task<IEnumerable<PermissionDto>> GetAllPermissionsAsync()
        {
            var permissions = await _unitOfWork.Permissions.GetAllAsync();
            return permissions.Select(p => new PermissionDto
            {
                Id = p.Id,
                Code = p.Code,
                Name = p.Name,
                Description = p.Description,
                CreatedAt = p.CreatedAt
            });
        }


    }
}