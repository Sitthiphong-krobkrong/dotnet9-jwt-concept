using dotnet9_jwt_concept.Core;
using dotnet9_jwt_concept.Helper;
using dotnet9_jwt_concept.Models.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace dotnet9_jwt_concept.Controllers
{
    [ApiController]
    [Authorize]
    [Route("[controller]")]
    public class UserController : Controller
    {
        private readonly IUserService _userService;
        private readonly JwtHelper _JwtHelper;
        public UserController(IUserService userService,JwtHelper jwtHelper)
        {
            _userService = userService;
            _JwtHelper = jwtHelper;
        }

        [HttpGet("users")]
        public async Task<IActionResult> getUser()
        {
            var users = await _userService.GetAllAsync();
            return Ok(ApiResponseFactory.Ok(
                   message: "get user success",
                   data: users
               )
           );
        }

        [HttpGet("verify-me")]
        public async Task<IActionResult> verifyMe()
        {
            var user = _JwtHelper.DecodeUserFromToken(HttpContext);
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
            var vaildUser = await _userService.GetByUserNameAsync(param.user_name);
            if (vaildUser != null)
            {
                return Conflict(ApiResponseFactory.Fail(
                   message: "user_name ซ้ำ กรุณาลองใหม่อีกครั้ง",
                   errorCode: "DuplicateUserName"
                ));
            }

            var createUser = await _userService.AddAsync(param);
            return Ok(ApiResponseFactory.Ok(
                   message: "สร้าง user สำเร็จ",
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
                    message: "user_id ไม่ถูกต้อง",
                    errorCode: "InvalidUserId"
                ));
            }

            var success = await _userService.UpdateAsync(param);

            if (!success)
            {
                return NotFound(ApiResponseFactory.Fail(
                    message: "ไม่พบข้อมูลผู้ใช้งาน",
                    errorCode: "NotfoundUserId"
                ));
            }

            return Ok(ApiResponseFactory.Ok(
                message: "อัปเดต user สำเร็จ",
                data: new { }
            ));
        }

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var userInfo = _JwtHelper.DecodeUserFromToken(HttpContext);
            if (userInfo == null)
            {
                return Unauthorized(ApiResponseFactory.Fail(
                    message: "Token invalid or expired",
                    errorCode: "InvalidToken"
                ));
            }

            int userId = userInfo.user_id;
            var success = await _userService.DeleteAsync(id, userId);
            if (!success)
            {
                return NotFound(ApiResponseFactory.Fail(
                    message: "ไม่พบข้อมูลผู้ใช้งาน",
                    errorCode: "NotfoundUserId"
                ));
            }

            return Ok(ApiResponseFactory.Ok(
                message: "ลบ user สำเร็จ",
                data: new { }
            ));
        }
    }
}
