using Microsoft.AspNetCore.Mvc;


namespace DormBuddy.Controllers {

    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthController(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpGet("isAuthenticated")]
        public IActionResult IsAuthenticated()
        {
            var isAuthenticated = User?.Identity?.IsAuthenticated;  // Check if user is authenticated
            return Ok(isAuthenticated);
        }
    }


}