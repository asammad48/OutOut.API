using Microsoft.AspNetCore.Mvc;

namespace OutOut.Controllers
{
    public class HomeController : ControllerBase
    {
        [HttpGet]
        public IActionResult Index()
        {
            return Ok("OutOut API is running successfully.");
        }
    }
}
