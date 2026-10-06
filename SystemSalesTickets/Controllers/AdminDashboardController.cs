using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SystemSalesTickets.Core.Enums;
using SystemSalesTickets.Core.Interfaces;

namespace SystemSalesTickets.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = nameof(UserRole.Manager))]
    public class AdminDashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;

        public AdminDashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        [HttpGet]
        public async Task<IActionResult> Get(CancellationToken cancellationToken)
        {
            var dashboard = await _dashboardService.GetAsync(cancellationToken);
            return Ok(dashboard);
        }
    }
}