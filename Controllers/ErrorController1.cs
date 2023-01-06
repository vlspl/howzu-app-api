using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace Howzu_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ErrorController1 : ControllerBase
    {
        [HttpGet("{code}")]
        public async Task<IActionResult> Get(int code)
        {
            dynamic Result = new JObject();  //Create root JSON Object
            Result.Status = false;
            Result.Msg = "Unauthorized access.";
            Result.StatusCode = code;
            return await Task.Run(() =>
            {

                return StatusCode(code, Result
                    );
                //{
                //    Status = false,
                //    Msg = "See the errors property for details.",
                //    Instance = HttpContext.Request.Path,
                //    StatusCode = code,
                //    Title = ((HttpStatusCode)code).ToString(),
                //    Type = "https://my.api.com/response"
                //}
            });
        }
    }
}

    

