namespace FoodConnect.Backend.Application.Commons.DTOs.Responses
{
    public class BaseResponse<T> where T : class
    {
        public bool Success { get; private set; }
        public string Message { get; private set; }
        public int StatusCode { get; private set; }
        public T? Data { get; private set; }
        public List<string>? Errors { get; private set; }

        public BaseResponse()
        {
            Success = false;
            Message = string.Empty;
            StatusCode = 400; // Default BadRequest
            Data = default;
            Errors = new List<string>();
        }

        public void SetInfo(bool success, string message, int statusCode = 400, List<string>? errors = null)
        {
            Success = success;
            Message = message;
            StatusCode = statusCode;
            Errors = errors ?? new List<string>();
        }

        public void SetData(T? Data)
        {
            this.Data = Data;
        }

        public BaseResponse<T> BuildSuccess(T? data, string message, int statusCode = 200)
        {
            SetInfo(true, message, statusCode);
            Data = data;
            return this;
        }
        
        public BaseResponse<T> BuildSuccess(T data)
        {
            return BuildSuccess(data, string.Empty, 200);
        }

        public BaseResponse<T> BuildSuccess(string message)
        {
            return BuildSuccess(default, message, 200);
        }

        public BaseResponse<T> BuildFail(string message, int statusCode = 400, List<string>? errors = null)
        {
            SetInfo(false, message, statusCode, errors);
            return this;
        }
        
        // Convenience methods for common status codes
        public BaseResponse<T> BuildNotFound(string message = "Resource not found")
        {
            return BuildFail(message, 404);
        }
        
        public BaseResponse<T> BuildUnauthorized(string message = "Unauthorized")
        {
            return BuildFail(message, 401);
        }
        
        public BaseResponse<T> BuildForbidden(string message = "Forbidden")
        {
            return BuildFail(message, 403);
        }
        
        public BaseResponse<T> BuildConflict(string message, List<string>? errors = null)
        {
            return BuildFail(message, 409, errors);
        }
    }
}
