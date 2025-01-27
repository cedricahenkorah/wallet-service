namespace WalletService.API.DTOs
{
    public class ApiResponse<T>
    {
        public ApiResponse(string code, string message)
        {
            Code = code;
            Message = message;
        }

        public ApiResponse(string code, string message, T? data)
        {
            Code = code;
            Message = message;
            Data = data;
        }

        public string Message { get; set; }

        public string Code { get; set; }
        public T? Data { get; set; }
    }
}
