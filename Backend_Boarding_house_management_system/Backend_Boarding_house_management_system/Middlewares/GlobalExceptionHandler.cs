using Backend_Boarding_house_management_system.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Backend_Boarding_house_management_system.Middlewares
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;
        private readonly IWebHostEnvironment _env;

        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger, IWebHostEnvironment env)
        {
            _logger = logger;
            _env = env;
        }

        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            _logger.LogError(exception, "Đã xảy ra lỗi: {Message}", exception.Message);

            // 1. Dùng Switch Expression để code gọn và dễ đọc hơn
            var (statusCode, title, errorCode) = exception switch
            {
                NotFoundException e => (StatusCodes.Status404NotFound, "Không tìm thấy dữ liệu (Not Found)", e.ErrorCode),
                BadRequestException e => (StatusCodes.Status400BadRequest, "Dữ liệu không hợp lệ (Bad Request)", e.ErrorCode),
                UnauthorizedException e => (StatusCodes.Status401Unauthorized, "Không có quyền truy cập (Unauthorized)", e.ErrorCode),
                ConflictException e => (StatusCodes.Status409Conflict, "Xung đột dữ liệu (Conflict)", e.ErrorCode),
                _ => (StatusCodes.Status500InternalServerError, "Lỗi máy chủ nội bộ (Internal Server Error)", null)
            };

            // 2. Logic hiển thị Detail thông minh hơn
            // Nếu là lỗi do mình tự định nghĩa (AppException) -> Luôn hiện message. 
            // Nếu là lỗi hệ thống (chia cho 0, null reference...) -> Chỉ hiện message ở Dev.
            bool isAppException = exception is AppException;
            string detailMessage = isAppException || _env.IsDevelopment()
                ? exception.Message
                : "Đã xảy ra lỗi hệ thống, vui lòng thử lại sau.";

            var problemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Detail = detailMessage,
                Instance = httpContext.Request.Path
            };

            if (errorCode != null)
            {
                problemDetails.Extensions["errorCode"] = errorCode;
            }

            problemDetails.Extensions["traceId"] = httpContext.TraceIdentifier;

            httpContext.Response.StatusCode = statusCode;
            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

            return true;
        }
    }
}