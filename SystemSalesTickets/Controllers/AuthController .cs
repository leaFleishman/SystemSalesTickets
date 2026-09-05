using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using SystemSalesTickets.Core.Interfaces;
using SystemSalesTickets.Core.Models;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly ILogger _logger;
    private readonly IUserService _userService;

    public AuthController(IConfiguration configuration, IUserService userService, ILogger<AuthController> logger)
    {
        _configuration = configuration;
        _userService = userService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> Login([FromBody] LoginModel loginModel)
    {
        _logger.LogInformation("Login request received at {RequestTime}", DateTime.Now);
        if (loginModel == null)
            return BadRequest();
        var user = await _userService.Login(loginModel);

        if (user == null)
        {
            _logger.LogWarning("Login failed");
            return Unauthorized();
        }


        var claims = new List<Claim>
    {
        new Claim(ClaimTypes.Role, "manager"),
        new Claim(ClaimTypes.Role, "user")
    };

        var key = _configuration.GetValue<string>("JWT:Key");

        if (string.IsNullOrEmpty(key))
            return StatusCode(500, "JWT Key is missing");

        var secretKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(key)
        );

        var signinCredentials = new SigningCredentials(
            secretKey,
            SecurityAlgorithms.HmacSha256
        );

        var token = new JwtSecurityToken(
            issuer: _configuration.GetValue<string>("JWT:Issuer"),
            audience: _configuration.GetValue<string>("JWT:Audience"),
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(6),
            signingCredentials: signinCredentials
        );

        var tokenString = new JwtSecurityTokenHandler()
            .WriteToken(token);
        _logger.LogInformation("Login for UserId {UserId} completed successfully", user.Id);
        return Ok(new
        {
            Token = tokenString
        });
    }
}