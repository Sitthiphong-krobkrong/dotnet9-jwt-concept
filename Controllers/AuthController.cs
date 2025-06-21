using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using dotnet9_jwt_concept.Helper;
using dotnet9_jwt_concept.Models.Core;
using System.IdentityModel.Tokens.Jwt;
using static dotnet9_jwt_concept.Models.AuthModels;

namespace dotnet9_jwt_concept.Controllers
{
    [ApiController]
    [Authorize]
    [Route("[controller]")]
    public class AuthController : Controller
    {

        private readonly JwtHelper _JwtHelper;
        public AuthController(JwtHelper JwtHelper)
        {
            _JwtHelper = JwtHelper;
        }

        // GET: /auth/health-check
        [HttpGet("health-check")]
        public Task<ActionResult<ApiResponse<object>>> IndexAsync()
        {
            // สมมติ payload เป็น anonymous object
            var payload = new { welcome = "Welcome to Auth API" };
            return Task.FromResult<ActionResult<ApiResponse<object>>>(
                Ok(ApiResponseFactory.Ok(payload, "เรียกดูสำเร็จ"))
            );
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<ActionResult<ApiResponse<object>>> AuthAsync([FromBody] Auth param)
        {
            // 1) Validation
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage);
                return BadRequest(
                    ApiResponseFactory.Fail(
                        errorCode: "ValidationFailed",
                        errorDetail: errors,
                        message: "ข้อมูลไม่ครบ"
                    )
                );
            }

            // 2) Business check
            if (param.Username != "admin" || param.Password != "1234")
            {
                return Unauthorized(
                    ApiResponseFactory.Fail(
                        errorCode: "InvalidCredentials",
                        errorDetail: "ชื่อผู้ใช้หรือรหัสผ่านไม่ถูกต้อง",
                        message: "ล็อกอินไม่สำเร็จ"
                    )
                );
            }

            // 3) Generate token
            var token = _JwtHelper.GenerateToken(param.Username, param.Password);
            var payload = new { token };

            // 4) Return success
            return Ok(
                ApiResponseFactory.Ok(
                    data: payload,
                    message: "Login successful"
                )
            );
        }

        // GET /auth/trigger-error
        [HttpGet("trigger-error")]
        public IActionResult TriggerError()
        {
            // โยน Exception ตรงนี้เลย
            throw new Exception("This is a test exception for middleware");
        }
    }
}
