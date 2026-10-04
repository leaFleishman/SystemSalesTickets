using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using SystemSalesTickets.Core.DTOs;
using SystemSalesTickets.Core.Enums;
using SystemSalesTickets.Core.Interfaces;

namespace SystemSalesTickets.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<UserController> _logger;
        private readonly IConfiguration _configuration;

        public UserController(IUserService userService, ILogger<UserController> logger, IConfiguration configuration)
        {
            _userService = userService;
            _logger = logger;
            _configuration = configuration;
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

        // Reverts a Manager back to a regular User.
        // Only the original administrator (email taken from "OriginalAdmin:Email",
        // default admin@example.com) may call this.
        [HttpPut("demote")]
        [Authorize(Roles = nameof(UserRole.Manager))]
        public async Task<ActionResult> MakeUserRegular([FromQuery] int id, CancellationToken cancellationToken)
        {
            var callerIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(callerIdClaim, out var callerId))
                return Unauthorized();

            var originalAdminEmail = _configuration["OriginalAdmin:Email"] ?? "admin@example.com";
            var caller = await _userService.GetUserById(callerId, cancellationToken);
            if (caller == null || !string.Equals(caller.Email, originalAdminEmail, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("User {UserId} tried to demote a manager but is not the original admin", callerId);
                return Forbid();
            }

            if (id == callerId)
                return BadRequest("The original administrator cannot be demoted.");

            var target = await _userService.GetUserById(id, cancellationToken);
            if (target == null)
            {
                _logger.LogWarning("User {UserId} was not found, demotion failed", id);
                return NotFound();
            }

            var user = await _userService.MakeUserRegular(id, cancellationToken);
            _logger.LogInformation("User {UserId} was demoted to regular user by {AdminId}", id, callerId);
            return Ok(user);
        }

    }
}