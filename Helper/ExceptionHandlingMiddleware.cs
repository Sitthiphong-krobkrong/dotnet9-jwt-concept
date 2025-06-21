using dotnet9_jwt_concept.Models.Core;
using System.Net;
using System.Text.Json;

namespace dotnet9_jwt_concept.Helper
{
    // Exception Middleware จับทุก Exception ไม่ต้องเขียน try/catch ซ้ำในแต่ละ Action
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;

        public ExceptionHandlingMiddleware(RequestDelegate next)
            => _next = next;

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            // สร้าง response ด้วย ApiResponse.Fail
            var apiError = ApiResponseFactory.Fail(
                errorCode: "InternalServerError",
                errorDetail: new { ex.Message, ex.StackTrace },
                message: "เกิดข้อผิดพลาดภายในเซิร์ฟเวอร์"
            );

            var payload = JsonSerializer.Serialize(apiError);

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            return context.Response.WriteAsync(payload);
        }
    }
}
