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

        public IActionResult Products()
        {
            return View();
        }

        public IActionResult SingleProduct()
        {
            return View();
        }
        
        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult Cart()
        {
            return View();
        }

        public IActionResult AdminProducts()
        {
            return View();
        }

        public IActionResult AdminEditProduct()
        {
            return View("AdminAddProduct");
        }

        public IActionResult AdminDeleteProduct()
        {
            return View();
        }

        public IActionResult AdminAddProduct()
        {
            return View();
        }

        public IActionResult AdminUsers()
        {
            return View();
        }

        public IActionResult AdminAddUser()
        {
            return View();
        }

        public IActionResult AdminDeleteUser()
        {
            return View();
        }

        public IActionResult AdminOrders()
        {
            return View();
        }
    }
}
