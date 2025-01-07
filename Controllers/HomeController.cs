using Microsoft.AspNetCore.Mvc;

namespace QuangNgoc.Recognizers.Controllers
{
    [Route("/")]
    [ApiController]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class HomeController : ControllerBase
    {
        [HttpGet]
        public IActionResult RedirectToSwagger()
        {
            // Redirects to the Swagger UI page
            return Redirect("/swagger/index.html");
        }
    }
}