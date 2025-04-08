using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DATN.Data;
using DATN.Model;
using DATN.Services;

namespace DATN.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly OderPitchDbContext _context;
        private readonly AuthService _authService;

        public AuthController(OderPitchDbContext context, AuthService authService)
        {
            _context = context;
            _authService = authService;
        }
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (await _context.Users.AnyAsync(u => u.Email == request.Email))
                return BadRequest("Email đã được sử dụng!");

            var user = new User
            {
                FullName = request.FullName,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                PasswordHash = _authService.HashPassword(request.PasswordHash),
                CreatedAt = DateTime.Now,
                Role = request.Role
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Đăng ký thành công!" });
        }

        // Đăng nhập
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == loginRequest.Email);
            if (user == null || !_authService.VerifyPassword(loginRequest.Password, user.PasswordHash))
                return Unauthorized("Email hoặc mật khẩu không đúng!");

            // Tạo JWT Token
            var accessToken = _authService.GenerateJwtToken(user);

            // Tạo Refresh Token
            var refreshToken = _authService.GenerateRefreshToken();
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                accessToken,
                refreshToken,
                role = user.Role // Trả về vai trò người dùng
            });
        }

        // Refresh Token
        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshRequest request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.RefreshToken == request.RefreshToken);
            if (user == null || user.RefreshTokenExpiry < DateTime.UtcNow)
                return Unauthorized("Refresh token không hợp lệ!");

            var accessToken = _authService.GenerateJwtToken(user);
            var newRefreshToken = _authService.GenerateRefreshToken();
            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
            await _context.SaveChangesAsync();

            return Ok(new { accessToken, refreshToken = newRefreshToken });
        }
    }

    // Model yêu cầu đăng nhập
    public class LoginRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    // Model yêu cầu refresh token
    public class RefreshRequest
    {
        public string RefreshToken { get; set; }
    }
    public class RegisterRequest
    {
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string Role { get; set; } 
    }
}
