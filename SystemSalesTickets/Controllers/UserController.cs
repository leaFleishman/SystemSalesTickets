using Microsoft.AspNetCore.Mvc;
using SystemSalesTicketsDomain.models;
using SystemSalesTicketsPresentation.Interfaces;

namespace SystemSalesTickets.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("AddUser")]
        public async Task<ActionResult<User>> AddUser([FromBody] User user)
        {
            var result = await _userService.AddUser(user);
            return Ok(result);
        }

        [HttpGet("GetAllUsers")]
        public async Task<ActionResult<IEnumerable<User>>> GetAllUsers()
        {
            var users = await _userService.GetAllUsers();
            return Ok(users);
        }

        [HttpGet("GetUserById")]
        public async Task<ActionResult<User>> GetUserById(int id)
        {
            var user = await _userService.GetUserById(id);

            if (user == null)
                return NotFound();

            return Ok(user);
        }

    }
}
