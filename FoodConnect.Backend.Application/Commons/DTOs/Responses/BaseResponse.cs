namespace FoodConnect.Backend.Application.Commons.DTOs.Responses
{
    public class BaseResponse<T> where T : class
    {
        public bool Success { get; private set; }
        public string Message { get; private set; }
        public T? Data { get; private set; }
        public List<string>? Errors { get; private set; }

        public BaseResponse()
        {
            Success = false;
            Message = string.Empty;
            Data = default;
            Errors = new List<string>();
        }

        public void SetInfo(bool success, string message, List<string>? errors = null)
        {
            Success = success;
            Message = message;
            Errors = errors ?? new List<string>();
        }

        public void SetData(T? Data)
        {
            this.Data = Data;
        }

        public BaseResponse<T> BuildSuccess(T? data, string message)
        {
            SetInfo(true, message);
            Data = data;
            return this;
        }
        public BaseResponse<T> BuildSuccess(T data)
        {
            return BuildSuccess(data, string.Empty);
        }

        public BaseResponse<T> BuildSuccess(string message)
        {
            return BuildSuccess(default, string.Empty);
        }

        public BaseResponse<T> BuildFail(string message, List<string>? errors = null)
        {
            SetInfo(false, message, errors);
            return this;
        }
    }
}
