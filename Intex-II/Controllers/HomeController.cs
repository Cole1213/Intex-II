using Intex_II.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Intex_II.Controllers
{
    public class HomeController : Controller
    {
        private ILegoRepository _repo;

        public HomeController(ILegoRepository temp)
        {
            _repo = temp;
        }

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
