using Intex_II.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Security.Cryptography.Xml;

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
            //Pass in the recommendations when we have them
            ViewBag.Recommendations = _repo.Products.Take(5).ToList();
            
            return View();
        }

        public IActionResult Products(List<string> categories = null, decimal? minPrice = null, decimal? maxPrice = null)
        {
            IQueryable<Product> productsQuery = _repo.Products; // Assuming _repo.Products is IQueryable<Product>

            // Apply filters based on categories and price if provided
            if (categories != null && categories.Any())
            {
                // Filter products that belong to any of the selected categories
                productsQuery = productsQuery.Where(p => categories.Contains(p.ProductCategory));
            }

            if (minPrice.HasValue)
            {
                productsQuery = productsQuery.Where(p => p.ProductPrice >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                productsQuery = productsQuery.Where(p => p.ProductPrice <= maxPrice.Value);
            }

            // Retrieve the filtered products
            var filteredProducts = productsQuery.ToList();

            ViewBag.Products = filteredProducts;

            ViewBag.Categories = _repo.Products
                                    .Select(p => p.ProductCategory)
                                    .Distinct()
                                    .ToList();

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
            return View();
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

        public IActionResult AdminAddUsers()
        {
            return View();
        }

        public IActionResult AdminDeleteUsers()
        {
            return View();
        }

        public IActionResult AdminOrders()
        {
            return View();
        }
    }
}
