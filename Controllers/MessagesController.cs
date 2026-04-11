using Microsoft.AspNetCore.Mvc;

namespace Twit.Controllers
{
    public class MessagesController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
