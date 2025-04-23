using System;
using System.ComponentModel.DataAnnotations;

namespace DATN.DTO
{
    // DTO cho thông tin người dùng trả về từ API
    public class UserDTO
    {
        public int UserID { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string Role { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    // DTO cho việc đăng ký người dùng mới
    public class UserCreateDTO
    {
        [Required]
        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [MaxLength(15)]
        public string PhoneNumber { get; set; } = string.Empty;

        [EmailAddress]
        [MaxLength(100)]
        public string? Email { get; set; }

        [Required]
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;

        [Required]
        [Compare("Password")]
        public string ConfirmPassword { get; set; } = string.Empty;

        public string Role { get; set; } = "User";
    }

    // DTO cho việc cập nhật thông tin người dùng
    public class UserUpdateDTO
    {
        [MaxLength(100)]
        public string? FullName { get; set; }

        [MaxLength(15)]
        public string? PhoneNumber { get; set; }

        [EmailAddress]
        [MaxLength(100)]
        public string? Email { get; set; }
    }

    // DTO cho việc đăng nhập
    public class UserLoginDTO
    {
        [Required]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }

    // DTO cho việc trả về thông tin xác thực
    public class UserAuthResponseDTO
    {
        public int UserID { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
    }

    // DTO cho việc đổi mật khẩu
    public class ChangePasswordDTO
    {
        [Required]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        public string NewPassword { get; set; } = string.Empty;

        [Required]
        [Compare("NewPassword")]
        public string ConfirmNewPassword { get; set; } = string.Empty;
    }
} 