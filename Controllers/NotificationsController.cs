using Microsoft.AspNetCore.Mvc;

namespace Twit.Controllers
{
    public class NotificationsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
