using Octokit.Internal;
using Refit;
using System.Text.Json.Serialization;

namespace dotnet9_jwt_concept.Models.Core
{
    // 1) Generic record สำหรับ API response (nullable Data)
    public record ApiResponse<T>
    {
        [JsonPropertyName("dateTime")]
        public DateTime Datetime { get; init; } = DateTime.UtcNow;

        [JsonPropertyName("success")]
        public bool Success { get; init; }

        [JsonPropertyName("message")]
        public string Message { get; init; } = string.Empty;

        [JsonPropertyName("data")]
        public T? Data { get; init; }
    }

    // 2) Rename static factory to avoid naming collision
    public static class ApiResponseFactory
    {
        /// <summary>
        /// สร้าง response เมื่อสำเร็จ
        /// </summary>
        public static ApiResponse<T> Ok<T>(T data, string message = "")
            => new()
            {
                Success = true,
                Message = message,
                Data = data
            };

        /// <summary>
        /// สร้าง response เมื่อเกิดข้อผิดพลาด
        /// </summary>
        public static ApiResponse<ErrorDetail> Fail(
            string errorCode,
            object? errorDetail = null,
            string message = ""
        )
            => new()
            {
                Success = false,
                Message = message,
                Data = new ErrorDetail(errorCode, errorDetail)
            };

        public record ErrorDetail(
            [property: JsonPropertyName("error")] string Code,
            [property: JsonPropertyName("errorDetail")] object? Detail
        );
    }
}
