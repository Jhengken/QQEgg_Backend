using Microsoft.AspNetCore.Mvc;
using QQEgg_Backend.DTO;
using QQEgg_Backend.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

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
        // GET: api/<ECPayController>
        //[HttpGet]
        //public IEnumerable<string> Get()
        //{
        //    return new string[] { "value1", "value2" };
        //}

        // GET api/<ECPayController>/5
        //[HttpGet("{id}")]
        //public string Get(int id)
        //{
        //    return "value";
        //}

        // POST api/<ECPayController>

        // PUT api/<ECPayController>/5
        //[HttpPut("{id}")]
        //public void Put(int id, [FromBody] string value)
        //{
        //}

        // DELETE api/<ECPayController>/5
        //[HttpDelete("{id}")]
        //public void Delete(int id)
        //{
        //}
    }
}
