using Microsoft.AspNetCore.Mvc;
using Template_WebAPI.Helpers.Errors;

namespace Template_WebAPI.Controllers
{
    [Route("errors/{code}")]
    public class ErrorsController : ControllerBase
    {
        [HttpGet]
        public IActionResult Error(int code)
        {
            return new ObjectResult(new ResponseAPI(code));
        }
    }
}
