using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SystemSalesTickets.Core.DTOs;
using SystemSalesTickets.Core.Enums;
using SystemSalesTickets.Core.Interfaces;
using SystemSalesTickets.Service.Service;

namespace SystemSalesTickets.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<UserController> _logger;

        public UserController(IUserService userService, ILogger<UserController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        [HttpPost("AddUser")]
        public async Task<ActionResult<UserDTO>> AddUser([FromBody] UserDTO user)
        {
            _logger.LogInformation("AddUser request received for Email {Email}", user?.Email);

            var result = await _userService.AddUser(user);
            _logger.LogInformation("AddUser completed successfully for UserId {UserId}", result?.Id);
            return Ok(result);
        }

        [HttpGet("GetAllUsers")]
        [Authorize(Roles = nameof(UserRole.Manager))]
        public async Task<ActionResult<IEnumerable<UserDTO>>> GetAllUsers()
        {
            _logger.LogInformation("GetAllUsers request received");
            var users = await _userService.GetAllUsers();
            _logger.LogInformation("GetAllUsers returned {Count} users", users?.Count() ?? 0);
            return Ok(users);
        }

        [HttpGet("GetUserById")]
        [Authorize(Roles = nameof(UserRole.Manager))]
        public async Task<ActionResult<UserDTO>> GetUserById(int id)
        {
            _logger.LogInformation("GetUserById request received for UserId {UserId}", id);
            var user = await _userService.GetUserById(id);
            if (user == null)
            {
                _logger.LogWarning("GetUserById: no user found for UserId {UserId}", id);
                return NotFound();
            }
            _logger.LogInformation("GetUserById succeeded for UserId {UserId}", id);
            return Ok(user);
        }
    }
}