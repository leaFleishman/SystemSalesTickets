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

        [HttpPost]
        public async Task<ActionResult<UserDTO>> AddUser([FromBody] RegisterRequestDTO user, CancellationToken cancellationToken)
        {
            _logger.LogInformation("AddUser request received for Email {Email}", user?.Email);

            var result = await _userService.AddUser(user, cancellationToken);
            _logger.LogInformation("AddUser completed successfully for UserId {UserId}", result?.Id);
            return Ok(result);
        }

        [HttpGet]
        [Authorize(Roles = nameof(UserRole.Manager))]
        public async Task<IActionResult> GetAllUsers([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("GetAllUsers request received");
            var users = await _userService.GetAllUsers(pageNumber, pageSize, cancellationToken);
            _logger.LogInformation("GetAllUsers returned page {PageNumber} with {Count} users", users.PageNumber, users.Data.Count());
            return Ok(users);
        }

        [HttpGet("{id:int}")]
        [Authorize(Roles = nameof(UserRole.Manager))]
        public async Task<ActionResult<UserDTO>> GetUserById([FromRoute] int id, CancellationToken cancellationToken)
        {
            _logger.LogInformation("GetUserById request received for UserId {UserId}", id);
            var user = await _userService.GetUserById(id, cancellationToken);
            if (user == null)
            {
                _logger.LogWarning("GetUserById: no user found for UserId {UserId}", id);
                return NotFound();
            }
            _logger.LogInformation("GetUserById succeeded for UserId {UserId}", id);
            return Ok(user);
        }
        [HttpPut]
        [Authorize(Roles = nameof(UserRole.Manager))]
        public async Task<ActionResult> MakeUserManager([FromQuery] int id, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Attempting to promote user {UserId} to Manager", id);

            var user = await _userService.MakeUserManager(id, cancellationToken);

            if (user == null)
            {
                _logger.LogWarning("User {UserId} was not found, promotion failed", id);
                return NotFound();
            }

            _logger.LogInformation("User {UserId} was successfully promoted to Manager", id);

            return Ok(user);
        }

    }
}