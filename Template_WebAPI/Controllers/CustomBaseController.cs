using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Template_WebAPI.Controllers
{
    public class CustomBaseController : Controller
    {
        [HttpGet]
        public Int64 getInfoCurrentUserId()
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var currentUserId = String.Empty;
            IEnumerable<Claim> claims = null;
            if (identity != null)
            {
                claims = identity.Claims;
                // or
                currentUserId = identity.FindFirst("uid").Value;

            };

            return Convert.ToInt64(currentUserId);
        }
    }
}
