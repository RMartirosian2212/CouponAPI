using System.Net;

namespace CouponApi.Models
{
    public class APIResponse
    {
        public bool IsSuccess { get; set; }
        public Object Result { get; set; }
        public HttpStatusCode StatusCode { get; set; }
        public List<string> ErrorMessage { get; set; } = new List<string>();
    }
}
