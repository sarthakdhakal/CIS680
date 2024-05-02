using Microsoft.AspNetCore.Mvc;
using NewDotnet.Code;
using NewDotnet.Models;
using System.Net;


namespace NewDotnet.Controllers
{
    [ApiController]
    [Route("api/public")]
    public class PublicApiController : ControllerBase
    {

        public PublicApiController()
        {
            
        }

        [HttpGet]
        [Route("test")]
        public IActionResult test()
        {
            string response = $"Hi there. The test was successful. The code version is {Services.APICodeVersion}.";        
            return Ok(new { message = response});
        }
        [HttpGet]
        [Route("version")]
        public IActionResult getVersion()
        {
            return Ok( new { error = 0, data = new { version = Services.APICodeVersion.ToString() } });
        }

        [HttpPost]
       
        [Route("advance")]
        public IActionResult manualUserAdvance([FromBody] Api_Advance parameters)
        {
            return BadRequest( new {status = HttpStatusCode.NotImplemented, error = 500, message = "Not yet implemented." });
        }
    }

    public class Api_Advance
    {
        public string starId { get; set; }
        public int playlistId { get; set; }
    }

}
