namespace FoodConnect.Backend.API.Models
{
    public class BaseResponse<T>
    {
        public bool Success { get; private set; }
        public string Message { get; private set; }
        public T? Data { get; private set; }
        public List<string>? Errors { get; private set; }

        public static BaseResponse<T> BuildSuccess(T data)
        {
            return new BaseResponse<T> { Success = true, Data = data };
        }
        public static BaseResponse<T> BuildSuccess(T data, string message)
        {
            return new BaseResponse<T> { Success = true, Data = data, Message = message };
        }

        public static BaseResponse<T> BuildFail(string message, List<string>? errors = null)
        {
            return new BaseResponse<T> { Success = false, Message = message, Errors = errors ?? new List<string>() };
        }
    }
}
