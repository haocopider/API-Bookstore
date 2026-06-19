using AutoMapper;
using Bookstore.Shared.Dtos;
using Bookstore.Shared.Helpers;
using Bookstore.Shared.Interfaces;
using Bookstore.Shared.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Newtonsoft.Json;

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

        public async Task<List<AuditLogDto>> GetRecentLogsAsync(int take = 50)
        {
            // Sử dụng Repository từ UnitOfWork
            var logs = await _unitOfWork.AuditLogs.FindAsync(a => a.TableName != "Role_Permissions", a => a.Admin);

            var list = logs
                .OrderByDescending(a => a.CreatedAt)
                .Take(take)
                .ToList();

            var result = new List<AuditLogDto>();

            foreach (var a in list)
            {
                Dictionary<string, string>? oldValues = null;
                Dictionary<string, string>? newValues = null;

                if (!string.IsNullOrEmpty(a.OldValues))
                {
                    try { oldValues = ParseToStringDict(a.OldValues); } catch { oldValues = null; }
                }
                if (!string.IsNullOrEmpty(a.NewValues))
                {
                    try { newValues = ParseToStringDict(a.NewValues); } catch { newValues = null; }
                }

                // Build a short human friendly summary
                string? summary = null;
                try
                {
                    if (a.ActionType.Equals("UPDATE", StringComparison.OrdinalIgnoreCase) && oldValues != null && newValues != null)
                    {
                        var changes = new List<string>();
                        foreach (var key in oldValues.Keys)
                        {
                            var oldV = oldValues.ContainsKey(key) ? oldValues[key] : null;
                            var newV = newValues.ContainsKey(key) ? newValues[key] : null;
                            if (oldV != newV)
                            {
                                changes.Add($"{key}: {oldV ?? "(null)"} → {newV ?? "(null)"}");
                            }
                        }
                        summary = changes.Any() ? string.Join("; ", changes).Truncate(200) : null;
                    }
                    else if (a.ActionType.Equals("DELETE", StringComparison.OrdinalIgnoreCase) && oldValues != null)
                    {
                        // show key fields to identify what was deleted
                        var keys = new List<string>();
                        foreach (var kvp in oldValues)
                        {
                            if (kvp.Key.Equals("Name", StringComparison.OrdinalIgnoreCase) || kvp.Key.Equals("Title", StringComparison.OrdinalIgnoreCase) || kvp.Key.Equals("Email", StringComparison.OrdinalIgnoreCase))
                                keys.Add($"{kvp.Key}: {kvp.Value}");
                        }
                        if (!keys.Any()) keys.AddRange(oldValues.Take(3).Select(k => $"{k.Key}: {k.Value}"));
                        summary = string.Join("; ", keys).Truncate(200);
                    }
                    else if ((a.ActionType.Equals("INSERT", StringComparison.OrdinalIgnoreCase) || a.ActionType.Equals("CREATE", StringComparison.OrdinalIgnoreCase)) && newValues != null)
                    {
                        var keys = new List<string>();
                        foreach (var kvp in newValues)
                        {
                            if (kvp.Key.Equals("Name", StringComparison.OrdinalIgnoreCase) || kvp.Key.Equals("Title", StringComparison.OrdinalIgnoreCase) || kvp.Key.Equals("Email", StringComparison.OrdinalIgnoreCase))
                                keys.Add($"{kvp.Key}: {kvp.Value}");
                        }
                        if (!keys.Any()) keys.AddRange(newValues.Take(3).Select(k => $"{k.Key}: {k.Value}"));
                        summary = string.Join("; ", keys).Truncate(200);
                    }
                }
                catch { summary = null; }

                result.Add(new AuditLogDto
                {
                    Id = a.Id,
                    AdminName = a.Admin != null ? a.Admin.FirstName + " " + a.Admin.LastName : "System",
                    ActionType = a.ActionType,
                    TableName = a.TableName,
                    RecordId = a.RecordId,
                    CreatedAt = a.CreatedAt,
                    OldValues = oldValues,
                    NewValues = newValues,
                    ChangesSummary = summary
                });
            }

            return result;
        }

        public async Task<bool> UndoActionAsync(int auditLogId, int currentAdminId)
        {
            // 1. Lấy thông tin log thông qua Repository
            var log = await _unitOfWork.AuditLogs.GetFirstOrDefaultAsync(a => a.Id == auditLogId);
            if (log == null) throw new Exception("Không tìm thấy nhật ký hệ thống.");

            // Khởi tạo Transaction từ UnitOfWork
            await _unitOfWork.BeginTransactionAsync();

            try
            {
                var oldValues = string.IsNullOrEmpty(log.OldValues)
                    ? new Dictionary<string, string>()
                    : ParseToStringDict(log.OldValues);


                string sqlQuery = string.Empty;
                var sqlParameters = new List<SqlParameter>();

                switch (log.ActionType.ToUpper())
                {
                    case "UPDATE":
                        if (oldValues == null || !oldValues.Any()) throw new Exception("Không có dữ liệu cũ để hoàn tác.");

                        var setClauses = new List<string>();
                        foreach (var kvp in oldValues)
                        {
                            if (kvp.Key.ToUpper() == "ID") continue;

                            setClauses.Add($"[{kvp.Key}] = @{kvp.Key}");
                            sqlParameters.Add(new SqlParameter($"@{kvp.Key}", kvp.Value ?? (object)DBNull.Value));
                        }

                        sqlParameters.Add(new SqlParameter("@RecordId", log.RecordId));
                        sqlQuery = $"UPDATE [{log.TableName}] SET {string.Join(", ", setClauses)} WHERE Id = @RecordId";
                        break;

                    case "DELETE":
                        if (oldValues == null || !oldValues.Any()) throw new Exception("Không có dữ liệu cũ để phục hồi.");

                        var columns = string.Join(", ", oldValues.Keys.Select(k => $"[{k}]"));
                        var parameters = string.Join(", ", oldValues.Keys.Select(k => $"@{k}"));

                        foreach (var kvp in oldValues)
                        {
                            sqlParameters.Add(new SqlParameter($"@{kvp.Key}", kvp.Value ?? (object)DBNull.Value));
                        }

                        sqlQuery = $"SET IDENTITY_INSERT [{log.TableName}] ON; " +
                                   $"INSERT INTO [{log.TableName}] ({columns}) VALUES ({parameters}); " +
                                   $"SET IDENTITY_INSERT [{log.TableName}] OFF;";
                        break;

                    case "INSERT":
                    case "CREATE":
                        sqlParameters.Add(new SqlParameter("@RecordId", log.RecordId));
                        sqlQuery = $"DELETE FROM [{log.TableName}] WHERE Id = @RecordId";
                        break;

                    default:
                        throw new Exception("Loại hành động không hỗ trợ hoàn tác.");
                }

                await _unitOfWork.ExecuteSqlRawAsync(sqlQuery, sqlParameters.ToArray());

                var undoLog = new AuditLog
                {
                    AdminId = currentAdminId,
                    ActionType = "UNDO_" + log.ActionType,
                    TableName = log.TableName,
                    RecordId = log.RecordId,
                    OldValues = log.NewValues,
                    NewValues = log.OldValues,
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.AuditLogs.AddAsync(undoLog);

                await _unitOfWork.CommitAsync();

                return true;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                throw new Exception($"Lỗi khi hoàn tác: {ex.Message}");
            }
        }

        private Dictionary<string, string> ParseToStringDict(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return new Dictionary<string, string>();

            // Try Newtonsoft.Json first for flexible parsing
            try
            {
                var obj = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
                if (obj == null) return new Dictionary<string, string>();

                var result = new Dictionary<string, string>();
                foreach (var kvp in obj)
                {
                    result[kvp.Key] = kvp.Value?.ToString();
                }
                return result;
            }
            catch
            {
                // Fallback to System.Text.Json
                var result = new Dictionary<string, string>();
                try
                {
                    using var doc = JsonDocument.Parse(json);
                    if (doc.RootElement.ValueKind == JsonValueKind.Object)
                    {
                        foreach (var prop in doc.RootElement.EnumerateObject())
                        {
                            result[prop.Name] = prop.Value.ValueKind == JsonValueKind.Null ? null : prop.Value.ToString();
                        }
                    }
                }
                catch
                {
                    // if both parsers fail, return empty dictionary
                }
                return result;
            }
        }
    }
}