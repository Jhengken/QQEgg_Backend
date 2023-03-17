using Microsoft.AspNetCore.Mvc;
using QQEgg_Backend.DTO;
using QQEgg_Backend.Models;


namespace QQEgg_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ECPayController : ControllerBase
    {
        [HttpPost]
        public string Post([FromBody] ECPayDetail detail)
        {
            var formString = new ECPayService().GetReturnValue(detail);
            return formString;
        }
        [HttpPost("ECPayReturn")]
        public string ECPayReturn()
        {
            ECPayResult result = new ECPayService().GetCallbackResult(Request.Form);
            //return result.ReceiveObj!;     //要看綠界回傳打開這行
            return result.ResponseECPay!;
        }
    }
}
