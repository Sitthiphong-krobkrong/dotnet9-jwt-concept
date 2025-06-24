using dotnet9_jwt_concept.Core;
using dotnet9_jwt_concept.Helper;
using dotnet9_jwt_concept.Models.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace dotnet9_jwt_concept.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class UserController : Controller
    {
        private readonly IUserService _userService;
        private readonly JwtHelper _JwtHelper;
        public UserController(IUserService userService,JwtHelper jwtHelper)
        {
            _userService = userService;
            _JwtHelper = jwtHelper;
        }

        // GET: /api/user/health-check
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


        [HttpGet("users")]
        public async Task<IActionResult> getUser()
        {
            var users = await _userService.ReadAllAsync();
            return Ok(ApiResponseFactory.Ok(
                   message: "get user success",
                   data: users
               )
           );
        }

        [HttpGet("verify-me")]
        public async Task<IActionResult> verifyMe()
        {
            var user = _JwtHelper.DecodeUserFromToken();
            if (user == null)
            {
                return Unauthorized(ApiResponseFactory.Fail(
                    message: "Token invalid or expired",
                    errorCode: "InvalidToken"
                ));
            }
            return Ok(ApiResponseFactory.Ok(
                message: "get user success",
                data: user
            ));
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateUser([FromBody] User param)
        {
            var vaildUser = await _userService.ReadByUserNameAsync(param.user_name);
            if (vaildUser != null)
            {
                return Conflict(ApiResponseFactory.Fail(
                   message: "user_name ซ้ำ กรุณาลองใหม่อีกครั้ง",
                   errorCode: "DuplicateUserName"
                ));
            }

            var createResult = await _userService.AddAsync(param);
            if (!createResult.Item2)
            {
                return BadRequest(ApiResponseFactory.Fail(
                    message: createResult.Item3,
                    errorCode: "InvalidToken"
                ));
            }
            return Ok(ApiResponseFactory.Ok(
                   message: createResult.Item3,
                   data: new { }
               )
           );
        }

        [HttpPatch("update")]
        public async Task<IActionResult> UpdateUser([FromBody] User param)
        {
            if (param?.user_id == null || param.user_id <= 0)
            {
                return BadRequest(ApiResponseFactory.Fail(
                    message: "ไม่พบข้อมูล user_id",
                    errorCode: "InvalidUserId"
                ));
            }

            var updateResult = await _userService.UpdateAsync(param);
            if (!updateResult.Item1)
            {
                return NotFound(ApiResponseFactory.Fail(
                    message: updateResult.Item2,
                    errorCode: "DuplicateUserName"
                ));
            }

            return Ok(ApiResponseFactory.Ok(
                message: updateResult.Item2,
                data: new { }
            ));
        }

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var deleteResult = await _userService.DeleteAsync(id);
            if (!deleteResult.Item1)
            {
                return NotFound(ApiResponseFactory.Fail(
                    message: deleteResult.Item2,
                    errorCode: "NotfoundUserId"
                ));
            }

            return Ok(ApiResponseFactory.Ok(
                message: deleteResult.Item2,
                data: new { }
            ));
        }
    }
}
