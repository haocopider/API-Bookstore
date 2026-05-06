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
                _configuration.GetSection("Jwt:Token").Value!));

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

    }
}
