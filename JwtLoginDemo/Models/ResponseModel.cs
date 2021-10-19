using JwtLoginDemo.Enums;

namespace JwtLoginDemo.Models
{
    public class ResponseModel
    {
        public ResponseModel(ResponseCode responseCode, string responseMessage = "", object data = null)
        {
            Code = responseCode;
            Message = responseMessage;
            Data = data;
        }

        public ResponseCode Code { get; set; }
        public string Message { get; set; }
        public object Data { get; set; }
    }
}