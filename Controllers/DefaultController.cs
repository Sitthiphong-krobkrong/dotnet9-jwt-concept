using dotnet9_jwt_concept.Models.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace dotnet9_jwt_concept.Controllers
{
    [AllowAnonymous]
    [ApiController]
    [Route("api/[controller]")]
    public class DefaultController : Controller
    {
        // GET: /api/default/health-check
        [HttpGet("health-check")]
        public async Task<ActionResult<object>> defaultAsync()
        {
            var payload = new
            {
                timeStamp = DateTime.UtcNow,
                statusCode = 200,
                message = "Api is running"
            };
            return Ok(payload);
        }
    }
}
