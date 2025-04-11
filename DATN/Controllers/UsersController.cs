using Microsoft.AspNetCore.Mvc;
using DATN.Repositories;
using DATN.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using BCrypt.Net;  // Thêm thư viện BCrypt.Net

namespace DATN.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserRepository _userRepository;

        public UsersController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        // GET: api/Users?page=1&pageSize=10
        [HttpGet]
        public async Task<ActionResult> GetUsers([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var users = await _userRepository.GetAllUsersAsync();
            var usersList = users.ToList();

            int totalCount = usersList.Count;
            int totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var paginatedUsers = usersList.Skip((page - 1) * pageSize).Take(pageSize);

            return Ok(new
            {
                items = paginatedUsers,
                totalCount,
                totalPages
            });
        }

        // GET api/Users/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(int id)
        {
            var user = await _userRepository.GetUserByIdAsync(id);
            if (user == null) return NotFound();
            return Ok(user);
        }

        // GET api/Users/email/{email}
        [HttpGet("email/{email}")]
        public async Task<ActionResult<User>> GetUserByEmail(string email)
        {
            var user = await _userRepository.GetUserByEmailAsync(email);
            if (user == null) return NotFound();
            return Ok(user);
        }
        [HttpGet("searchTerm/search")]
        public async Task<ActionResult<IEnumerable<User>>> SearchUsersByEmail([FromQuery] string searchTerm)
        {
            // Kiểm tra nếu searchTerm có giá trị
            if (string.IsNullOrEmpty(searchTerm))
            {
                return BadRequest("Search term is required.");
            }

            // Tìm kiếm email chứa cụm từ (sử dụng Contains)
            var users = await _userRepository.GetUsersByEmailContainsAsync(searchTerm);

            if (users == null)
            {
                return NotFound("No users found.");
            }

            return Ok(users);
        }


        // POST api/Users
        [HttpPost]
        public async Task<IActionResult> PostUser([FromBody] User user)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // Mã hóa mật khẩu trước khi lưu
                if (!string.IsNullOrEmpty(user.PasswordHash))
                {
                    user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);
                }

                await _userRepository.AddUserAsync(user);
                return CreatedAtAction("GetUser", new { id = user.UserID }, user);
            }
            catch (Exception ex)
            {
                // Log the exception (ex) here
                return StatusCode(500, "Internal server error");
            }
        }

        // PUT api/Users/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] User user)
        {
            if (id != user.UserID) return BadRequest();

            // Mã hóa mật khẩu nếu có thay đổi
            if (!string.IsNullOrEmpty(user.PasswordHash))
            {
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);
            }

            try
            {
                await _userRepository.UpdateUserAsync(user);
                return NoContent();
            }
            catch (Exception ex)
            {
                // Log the exception (ex) here
                return StatusCode(500, "Internal server error");
            }
        }

        // DELETE api/Users/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            await _userRepository.DeleteUserAsync(id);
            return NoContent();
        }
    }
}
