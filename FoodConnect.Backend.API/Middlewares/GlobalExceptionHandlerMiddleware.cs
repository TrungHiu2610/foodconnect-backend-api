using FoodConnect.Backend.API.Models;
using FoodConnect.Backend.Application.Commons.Exceptions;
using System.Net;
using System.Text.Json;

namespace FoodConnect.Backend.API.Middlewares
{
    public class GlobalExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

        public GlobalExceptionHandlerMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

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

        private Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var actualException = exception.InnerException ?? exception;
            var statusCode = HttpStatusCode.InternalServerError;
            BaseResponse<object> response;

            switch (actualException)
            {
                case ValidationException validationException:
                    statusCode = HttpStatusCode.BadRequest; 

                    var validationErrors = validationException.Errors.SelectMany(kvp => kvp.Value).ToList();
                    response = BaseResponse<object>.BuildFail("One or more validation errors occurred.", validationErrors);
                    break;

                case BadRequestException badRequestException:
                    statusCode = HttpStatusCode.BadRequest; // 400
                    response = BaseResponse<object>.BuildFail(badRequestException.Message);
                    break;

                case NotFoundException notFoundException:
                    statusCode = HttpStatusCode.NotFound; // 404
                    response = BaseResponse<object>.BuildFail(notFoundException.Message);
                    break;

                default:
                    _logger.LogError(exception, "An unhandled exception has occurred.");
                    response = BaseResponse<object>.BuildFail("An internal server error has occurred. Please try again later.");
                    break;
            }

            var payload = JsonSerializer.Serialize(response, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;
            return context.Response.WriteAsync(payload);
        }
    }
}
