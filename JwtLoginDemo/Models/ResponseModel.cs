using JwtLoginDemo.Enums;

namespace JwtLoginDemo.Models
{
    public class ResponseModel
    {
        public ResponseModel(ResponseCode code, string message = "", object data = null)
        {
            Code = code;
            Message = message;
            Data = data;
        }

        public ResponseCode Code { get; set; }
        public string Message { get; set; }
        public object Data { get; set; }
    }
}