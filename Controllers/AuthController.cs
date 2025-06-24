using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using dotnet9_jwt_concept.Helper;
using dotnet9_jwt_concept.Models.Core;
using System.IdentityModel.Tokens.Jwt;
using static dotnet9_jwt_concept.Models.AuthModels;
using dotnet9_jwt_concept.Core;

namespace dotnet9_jwt_concept.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class AuthController : Controller
    {
        private readonly IUserService _userService;
        private readonly JwtHelper _JwtHelper;
        public AuthController(JwtHelper JwtHelper, IUserService userService)
        {
            _userService = userService;
            _JwtHelper = JwtHelper;
        }

        // GET: /auth/health-check
        [AllowAnonymous]
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


        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<ActionResult<ApiResponse<object>>> AuthAsync([FromBody] AuthRequest param)
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

            var checkUser = await _userService.ReadByUserPassAsync(param.Username,param.Password);
            if (checkUser == null)
            {
                return Unauthorized(
                    ApiResponseFactory.Fail(
                        errorCode: "UserNotFound",
                        errorDetail: "ชื่อผู้ใช้หรือรหัสผ่านไม่ถูกต้อง",
                        message: "ล็อกอินไม่สำเร็จ"
                    )
                );
            }

            // 3) Generate token
            var genTokenResult = await _JwtHelper.GenerateTokenPair(checkUser);
            var payload = new 
            { 
                token = genTokenResult.AccessToken,
                refreshToken = genTokenResult.RefreshToken,
            };

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
