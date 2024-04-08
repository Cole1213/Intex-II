using Intex_II.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Intex_II.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        
        public IActionResult Privacy()
        {
            return View();
        }
    }
}
