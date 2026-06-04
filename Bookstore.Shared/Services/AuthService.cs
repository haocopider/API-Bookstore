using AutoMapper;
using Bookstore.Shared.Dtos;
using Bookstore.Shared.Helpers;
using Bookstore.Shared.Interfaces;
using Bookstore.Shared.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Bookstore.Shared.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;

        public AuthService(IUnitOfWork unitOfWork, IMapper mapper, IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _configuration = configuration;
        }

        public async Task<bool> RegisterAsync(RegisterRequest request)
        {
            var existingUser = await _unitOfWork.Users.GetFirstOrDefaultAsync(u => u.Email == request.Email);
            if (existingUser != null)
            {
                return false; 
            }
            var newUser = _mapper.Map<User>(request);
            newUser.PasswordHash = PasswordHelper.Hash(request.Password);   
            await _unitOfWork.Users.AddAsync(newUser);
            await _unitOfWork.CommitAsync();

            return true;
        }

        public async Task<AuthResponse?> LoginAsync(LoginRequest request)
        {
            var user = await _unitOfWork.Users.GetFirstOrDefaultAsync(u => u.Email == request.Email);

            if (user == null || !PasswordHelper.Verify(request.Password, user.PasswordHash))
            {
                return null;
            }

            if (!user.IsActive)
            {
                throw new Exception("Tài khoản của bạn đã bị khóa.");
            }

            string token = CreateToken(user);

            var userInfoDto = _mapper.Map<UserInfoDto>(user);

            return new AuthResponse
            {
                Token = token,
                User = userInfoDto
            };
        }

        public async Task<bool> ChangePasswordAsync(int userId, ChangePasswordDto dto)
        {
            var user = await _unitOfWork.Users.GetFirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return false;
            }
            bool isOldPasswordValid = PasswordHelper.Verify(dto.OldPassword, user.PasswordHash);

            if (!isOldPasswordValid)
            {
                return false;
            }
            
            user.PasswordHash = PasswordHelper.Hash(dto.NewPassword);

            _unitOfWork.Users.Update(user);
            var result = await _unitOfWork.CommitAsync();

            return result > 0;
        }
        public async Task<UserInfoDto?> GetProfileAsync(int userId)
        {
            var user = await _unitOfWork.Users.GetFirstOrDefaultAsync(u => u.Id == userId);

            if (user == null) return null;

            return _mapper.Map<UserInfoDto>(user);
        }

        public async Task<bool> UpdateProfileAsync(int userId, UpdateProfileDto dto)
        {
            var user = await _unitOfWork.Users.GetFirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return false;
            }

            user.FullName = dto.FullName;
            user.PhoneNumber = dto.PhoneNumber;
            user.Address = dto.Address;

            _unitOfWork.Users.Update(user);
            var result = await _unitOfWork.CommitAsync();
            return result > 0;
        }

        private string CreateToken(User user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                _configuration.GetSection("Jwt:Key").Value!));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.FullName)
            };

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddDays(7),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<string> LoginAdminAsync(string username, string password)
        {
            // 1. Kiểm tra email & password (giả sử đã validate thành công)
            var admin = await _unitOfWork.Admins.FindAsync(
                a => a.UserName == username,
                a => a.Role // Include Role
            );
            var currentAdmin = admin.FirstOrDefault();

            if (currentAdmin == null /* || !VerifyPassword(password, currentAdmin.PasswordHash) */)
                throw new Exception("Sai thông tin đăng nhập.");

            // 2. Lấy danh sách các Permission của Role này
            var rolePermissions = await _unitOfWork.RolePermissions.FindAsync(
                rp => rp.RoleId == currentAdmin.RoleId,
                rp => rp.Permission // Include bảng Permission để lấy mã quyền
            );

            // 3. Khởi tạo danh sách Claims cho JWT
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, currentAdmin.Id.ToString()),
                new Claim(ClaimTypes.Name, currentAdmin.UserName),
                new Claim(ClaimTypes.Role, currentAdmin.Role.Code)
            };

            // 4. Thêm từng mã Permission vào Claims
            foreach (var rp in rolePermissions)
            {
                if (rp.Permission != null)
                {
                    // Ví dụ mã quyền: "CREATE_BOOK", "MANAGE_PROMOTIONS"
                    claims.Add(new Claim("Permission", rp.Permission.Code));
                }
            }

            // 5. Sinh JWT Token bằng JwtSecurityTokenHandler (Sử dụng config hiện tại của bạn)
            var token = GenerateJwtToken(claims);
            return token;
        }

        private string GenerateJwtToken(IEnumerable<Claim> claims)
        {
            // 1. Lấy chuỗi bí mật từ cấu hình (appsettings.json)
            // Lưu ý: Chuỗi SecretKey phải đủ dài và phức tạp (ít nhất 16 ký tự)
            var secretKey = _configuration["Jwt:Key"];

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

            // 2. Định nghĩa thuật toán mã hóa
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // 3. Khởi tạo Token Descriptor với các thông số cài đặt
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                // Thời gian sống của Token (Ví dụ: 2 giờ)
                Expires = DateTime.UtcNow.AddHours(48),
                // Có thể lấy Issuer và Audience từ appsettings.json nếu cần
                Issuer = _configuration["JwtSettings:Issuer"],
                Audience = _configuration["JwtSettings:Audience"],
                SigningCredentials = creds
            };

            // 4. Tạo và trả về chuỗi Token
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }

        public async Task<bool> UpdateFcmToken(int userId, string fcmToken)
        {
            var user = await _unitOfWork.Users.GetFirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                return false;
            }
            user.FcmToken = fcmToken;
            _unitOfWork.Users.Update(user);
            var result = await _unitOfWork.CommitAsync();
            return result > 0;
        }
    }
}
