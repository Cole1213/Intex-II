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

        [HttpPost]
        public IActionResult Cart(Cart cart)
        {
            _repo.RemoveCart(cart);

            return RedirectToAction("Cart");
        }

        public IActionResult AdminProducts()
        {
            ViewBag.Products = _repo.Products.OrderBy(x => x.ProductName).ToList();

            return View();
        }

        [HttpGet]
        public IActionResult AdminEditProduct(int productId)
        {
            var recordToEdit = _repo.Products.Single(x => x.ProductId == productId);

            return View("AdminAddProduct", recordToEdit);
        }

        [HttpPost]
        public IActionResult AdminEditProduct(Product product)
        {
            _repo.UpdateProduct(product);

            return RedirectToAction("AdminProducts");
        }

        [HttpGet]
        public IActionResult AdminDeleteProduct(int productId)
        {
            var recordToDelete = _repo.Products.Single(y => y.ProductId == productId);

            return View(recordToDelete);
        }

        [HttpPost]
        public IActionResult AdminDeleteProduct(Product product)
        {
            _repo.DeleteProduct(product);

            return RedirectToAction("AdminProducts");
        }

        [HttpGet]
        public IActionResult AdminAddProduct()
        {
            return View();
        }

        [HttpPost]
        public IActionResult AdminAddProduct(Product product) 
        {
            _repo.AddProduct(product);

            return RedirectToAction("AdminProducts");
        }

        public IActionResult AdminUsers(int page = 1, int pageSize = 10)
        {
            // Calculate the number of items to skip based on the page number and page size
            int skip = (page - 1) * pageSize;

            ViewBag.Users = _repo.AspNetUsers
                .SelectMany(u => u.Roles, (user, role) => new { user, role.Name })
                .ToList();

            // Count the total number of customers
            int totalUsers = _repo.AspNetUsers.Count();

            // Calculate the total number of pages
            int totalPages = (int)Math.Ceiling((double)totalUsers / pageSize);

            // Pass the customers and pagination information to the view
            ViewBag.TotalPages = totalPages;
            ViewBag.CurrentPage = page;

            return View();
        }

        [HttpGet]
        public IActionResult AdminDeleteUser(string userId)
        {
            var recordToDelete = _repo.AspNetUsers.Single(y => y.Id.Equals(userId));

            return View(recordToDelete);
        }

        [HttpPost]
        public IActionResult AdminDeleteUser(AspNetUser user)
        {
            _repo.DeleteUser(user);

            return RedirectToAction("AdminUsers");
        }

        [HttpGet]
        public IActionResult AdminAddUser()
        {
            return View();
        }

        [HttpPost]
        public IActionResult AdminAddUser(Customer customer)
        {
            _repo.AddUser(customer);

            return RedirectToAction("AdminUsers");
        }

        public IActionResult AdminOrders(int page = 1, int pageSize = 10)
        {
            // Calculate the number of items to skip based on the page number and page size
            int skip = (page - 1) * pageSize;

            ViewBag.Orders = _repo.Orders.OrderByDescending(x => x.TransactionDate).Take(pageSize).ToList();

            // Count the total number of customers
            int totalOrders = 100;

            // Calculate the total number of pages
            int totalPages = (int)Math.Ceiling((double)totalOrders / pageSize);

            // Pass the customers and pagination information to the view
            ViewBag.TotalPages = totalPages;
            ViewBag.CurrentPage = page;

            return View();
        }

        public IActionResult Dashboard()
        {
            return View();
        }

        [HttpGet]
        public IActionResult SubmitOrder(int customerId = 1)
        {
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

            ViewBag.CustomerId = customerId;

            ViewBag.EntryModes = _repo.Orders
                                    .Select(p => p.EntryMode)
                                    .Distinct()
                                    .ToList();

            ViewBag.TransactionTypes = _repo.Orders
                                    .Select(p => p.TransactionType)
                                    .Distinct()
                                    .ToList();

            ViewBag.TransactionCardTypes = _repo.Orders
                                    .Select(p => p.TransactionTypeOfCard)
                                    .Distinct()
                                    .ToList();

            return View();
        }
        
        [HttpPost]
        public IActionResult SubmitOrder(Order order)
        {
            Order addedOrder = _repo.AddOrder(order);

            List<Cart> CartItems = _repo.Carts.Where(x => x.CustomerId == order.CustomerId).ToList();

            foreach (var item in CartItems) 
            {
                LineItem lineItem = new LineItem
                {
                    TransactionId = addedOrder.TransactionId, 
                    ProductId = item.ProductId, 
                    Quantity = item.ItemQuantity
                };

                _repo.AddLineItem(lineItem);

                _repo.RemoveCart(item);
            }

            return RedirectToAction("Index");
        }
    }
}
