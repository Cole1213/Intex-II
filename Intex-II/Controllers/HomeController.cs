using Azure;
using Intex_II.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Security.Cryptography.Xml;
using Microsoft.AspNetCore.Authorization;

namespace Intex_II.Controllers
{
    public class HomeController : Controller
    {
        private ILegoRepository _repo;

        public HomeController(ILegoRepository temp)
        {
            _repo = temp;
        }

        [HttpGet]
        public IActionResult Index()
        {
            ViewBag.CartItemCount = 2;
            //Pass in the recommendations when we have them
            ViewBag.Recommendations = _repo.Products.Take(5).ToList();

            return View();
        }
        
        [HttpPost]
        public IActionResult Index(Cart cart)
        {
            _repo.AddCart(cart);

            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Products(List<string> categories = null, decimal? minPrice = null, decimal? maxPrice = null)
        {
            IQueryable<Product> productsQuery = _repo.Products; // Assuming _repo.Products is IQueryable<Product>

            // Apply filters based on categories and price if provided
            if (categories != null && categories.Any())
            {
                // Filter products that belong to any of the selected categories
                productsQuery = productsQuery.Where(p => categories.Contains(p.ProductCategorySimple));
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

            // Pass in categories for the checkbox filters
            ViewBag.Categories = _repo.Products
                                    .Select(p => p.ProductCategorySimple)
                                    .Distinct()
                                    .ToList();

            return View();
        }

        [HttpPost]
        public IActionResult Products(Cart cart)
        {
            _repo.AddCart(cart);

            return RedirectToAction("Products");
        }

        [HttpGet]
        public IActionResult SingleProduct(int productId)
        {
            ViewBag.Products = _repo.Products.FirstOrDefault(p => p.ProductId == productId);

            return View();
        }

        [HttpPost]
        public IActionResult SingleProduct(Cart cart)
        {
            _repo.AddCart(cart);

            return RedirectToAction("SingleProduct", new { productId = cart.ProductId });
        }
        
        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult About()
        {
            return View();
        }
        [Authorize(Roles = "Customer")]
        [HttpGet]
        public IActionResult Cart(int customerId = 1)
        {
            //Pass in actual cart items later
            ViewBag.CartItems = (from Carts in _repo.Carts
                                 join Products in _repo.Products
                                 on Carts.ProductId equals Products.ProductId
                                 where Carts.CustomerId.Equals(customerId)
                                 select new
                                 {
                                     CustomerId = Carts.CustomerId,
                                     ProductId = Carts.ProductId,
                                     Quantity = Carts.ItemQuantity,
                                     TotalPrice = Carts.TotalPrice,
                                     ProductName = Products.ProductName,
                                     ProductYear = Products.ProductYear,
                                     ProductNumParts = Products.ProductNumParts,
                                     ProductPrice = Products.ProductPrice,
                                     ProductImage = Products.ProductImage,
                                     ProductDescription = Products.ProductDescription,
                                     ProductCategorySimple = Products.ProductCategorySimple
                                 }).ToList();

            return View();
        }
        [Authorize(Roles = "Customer")]
        [HttpPost]
        public IActionResult Cart(Cart cart)
        {
            _repo.RemoveCart(cart);

            return RedirectToAction("Cart");
        }
        [Authorize(Roles = "Admin")]
        public IActionResult AdminProducts()
        {
            ViewBag.Products = _repo.Products.OrderBy(x => x.ProductName).ToList();

            return View();
        }
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult AdminEditProduct(int productId)
        {
            var recordToEdit = _repo.Products.Single(x => x.ProductId == productId);

            return View("AdminAddProduct", recordToEdit);
        }
        
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public IActionResult AdminEditProduct(Product product)
        {
            _repo.UpdateProduct(product);

            return RedirectToAction("AdminProducts");
        } 
        
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult AdminDeleteProduct(int productId)
        {
            var recordToDelete = _repo.Products.Single(y => y.ProductId == productId);

            return View(recordToDelete);
        }
        
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public IActionResult AdminDeleteProduct(Product product)
        {
            _repo.DeleteProduct(product);

            return RedirectToAction("AdminProducts");
        }
        
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult AdminAddProduct()
        {
            return View();
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public IActionResult AdminAddProduct(Product product) 
        {
            _repo.AddProduct(product);

            return RedirectToAction("AdminProducts");
        }

        [Authorize(Roles = "Admin")]
        public IActionResult AdminUsers(int page = 1, int pageSize = 100)
        {
            // Calculate the number of items to skip based on the page number and page size
            int skip = (page - 1) * pageSize;

            // Retrieve a page of customers from the repository
            var customers = _repo.Customers.OrderBy(x => x.CustomerFname).Skip(skip).Take(pageSize).ToList();

            // Count the total number of customers
            int totalCustomers = _repo.Customers.Count();

            // Calculate the total number of pages
            int totalPages = (int)Math.Ceiling((double)totalCustomers / pageSize);

            // Pass the customers and pagination information to the view
            ViewBag.Customers = customers;
            ViewBag.TotalPages = totalPages;
            ViewBag.CurrentPage = page;

            return View();
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult AdminDeleteUser(int customerId)
        {
            var recordToDelete = _repo.Customers.Single(y => y.CustomerId == customerId);

            return View(recordToDelete);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public IActionResult AdminDeleteUser(Customer customer)
        {
            _repo.DeleteUser(customer);

            return RedirectToAction("AdminUsers");
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult AdminAddUser()
        {
            return View();
        }
        
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public IActionResult AdminAddUser(Customer customer)
        {
            _repo.AddUser(customer);

            return RedirectToAction("AdminUsers");
        }
        
        [Authorize(Roles = "Admin")]
        public IActionResult AdminOrders()
        {
            return View();
        }

        [Authorize(Roles = "Customer")]
        public IActionResult Dashboard()
        {
            return View();
        }
    }
}
