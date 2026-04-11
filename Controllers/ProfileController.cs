using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Twit.Models;

namespace Twit.Controllers;

public class ProfileController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}