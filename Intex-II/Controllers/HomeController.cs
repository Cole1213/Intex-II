using Azure;
using Intex_II.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Security.Cryptography.Xml;
using Microsoft.AspNetCore.Authorization;
using Microsoft.CodeAnalysis;

namespace Intex_II.Controllers
{
    public class HomeController : Controller
    {
        private ILegoRepository _repo;
        private SignInManager<IdentityUser> _signInManager;

        public HomeController(ILegoRepository temp, SignInManager<IdentityUser> signInManager)
        {
            _repo = temp;
            _signInManager = signInManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            ViewBag.CartItemCount = 2;
            //Pass in the recommendations when we have them
            ViewBag.Recommendations = _repo.Products.Take(5).ToList();
            
            string userName = null; // Initialize userId with null

            var customer = HttpContext.User;

            if (customer.Identity.IsAuthenticated)
            {
                var user = await _signInManager.UserManager.GetUserAsync(customer);

                userName = user.UserName;
            }

            ViewBag.CustomerId = _repo.Customers.Where(x => x.CustomerEmail.Equals(userName)).Select(x => x.CustomerId).FirstOrDefault();

            return View();
        }

        [HttpPost]
        public IActionResult Index(Cart cart)
        {
            // Check if the product already exists in the cart for the current customer
            var existingCartItem = _repo.Carts
                .FirstOrDefault(c => c.CustomerId == cart.CustomerId && c.ProductId == cart.ProductId);

            if (existingCartItem != null)
            {
                // Update the quantity and total price of the existing cart item
                Cart newCartItem = new Cart
                {
                    CustomerId = existingCartItem.CustomerId,
                    ProductId = existingCartItem.ProductId,
                    ItemQuantity = existingCartItem.ItemQuantity + 1,
                    TotalPrice = existingCartItem.TotalPrice
                };

                _repo.RemoveCart(existingCartItem);

                //_repo.AddCart(newCartItem);
            }
            else
            {
                // If the product doesn't exist in the cart, add a new entry
                //_repo.AddCart(cart);
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Products(List<string> categories = null, decimal? minPrice = null, decimal? maxPrice = null)
        {
            string userName = null; // Initialize userId with null

            var customer = HttpContext.User;

            if (customer.Identity.IsAuthenticated)
            {
                var user = await _signInManager.UserManager.GetUserAsync(customer);

                userName = user.UserName;
            }

            ViewBag.CustomerId = _repo.Customers.Where(x => x.CustomerEmail.Equals(userName)).Select(x => x.CustomerId).FirstOrDefault();

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
            var existingCartItem = _repo.Carts
                .FirstOrDefault(c => c.CustomerId == cart.CustomerId && c.ProductId == cart.ProductId);

            if (existingCartItem != null)
            {
                // Update the quantity and total price of the existing cart item
                Cart newCartItem = new Cart
                {
                    CustomerId = existingCartItem.CustomerId,
                    ProductId = existingCartItem.ProductId,
                    ItemQuantity = existingCartItem.ItemQuantity + 1,
                    TotalPrice = existingCartItem.TotalPrice
                };

                _repo.RemoveCart(existingCartItem);

                _repo.AddCart(newCartItem);
            }
            else
            {
                // If the product doesn't exist in the cart, add a new entry
                _repo.AddCart(cart);
            }

            return RedirectToAction("Products");
        }

        [HttpGet]
        public async Task<IActionResult> SingleProduct(int productId)
        {
            string userName = null; // Initialize userId with null

            var customer = HttpContext.User;

            if (customer.Identity.IsAuthenticated)
            {
                var user = await _signInManager.UserManager.GetUserAsync(customer);

                userName = user.UserName;
            }

            ViewBag.CustomerId = _repo.Customers.Where(x => x.CustomerEmail.Equals(userName)).Select(x => x.CustomerId).FirstOrDefault();

            ViewBag.Products = _repo.Products.FirstOrDefault(p => p.ProductId == productId);

            ViewBag.SimilarProducts = (from Recommendations in _repo.Recommendations
                                       join Products in _repo.Products
                                       on Recommendations.RecommendedProductId equals Products.ProductId
                                       where Recommendations.ProductId == productId
                                       orderby Recommendations.Rank
                                       select new
                                       {
                                           ProductId = Products.ProductId,
                                           ProductName = Products.ProductName,
                                           ProductDescription = Products.ProductDescription,
                                           ProductPrice = Products.ProductPrice,
                                           ProductCategory = Products.ProductCategory,
                                           ProductImage = Products.ProductImage,
                                           ProductCategorySimple = Products.ProductCategorySimple,
                                           Rank = Recommendations.Rank
                                       }).ToList();

            return View();
        }
        
        [Authorize(Roles = "Customer")]
        [HttpPost]
        public IActionResult SingleProduct(Cart cart)
        {
            var existingCartItem = _repo.Carts
                .FirstOrDefault(c => c.CustomerId == cart.CustomerId && c.ProductId == cart.ProductId);

            if (existingCartItem != null)
            {
                // Update the quantity and total price of the existing cart item
                Cart newCartItem = new Cart
                {
                    CustomerId = existingCartItem.CustomerId,
                    ProductId = existingCartItem.ProductId,
                    ItemQuantity = existingCartItem.ItemQuantity + 1,
                    TotalPrice = existingCartItem.TotalPrice
                };

                _repo.RemoveCart(existingCartItem);

                _repo.AddCart(newCartItem);
            }
            else
            {
                // If the product doesn't exist in the cart, add a new entry
                _repo.AddCart(cart);
            }

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
        public async Task<IActionResult> Cart()
        {
            var signedInUser = HttpContext.User;

            var user = await _signInManager.UserManager.GetUserAsync(signedInUser);

            var userName = user.UserName;

            int customerId = _repo.Customers.Where(x => x.CustomerEmail.Equals(userName)).Select(x => x.CustomerId).FirstOrDefault();

            ViewBag.CustomerId = customerId;

            //Pass in actual cart items later
            ViewBag.CartItems = (from Carts in _repo.Carts
                                 join Products in _repo.Products
                                 on Carts.ProductId equals Products.ProductId
                                 where Carts.CustomerId.Equals(customerId)
                                 select new
                                 {
                                     CustomerId = Carts.CustomerId,
                                     ProductId = Carts.ProductId,
                                     ItemQuantity = Carts.ItemQuantity,
                                     TotalPrice = Carts.TotalPrice,
                                     ProductName = Products.ProductName,
                                     ProductYear = Products.ProductYear,
                                     ProductNumParts = Products.ProductNumParts,
                                     ProductPrice = Products.ProductPrice,
                                     ProductImage = Products.ProductImage,
                                     ProductDescription = Products.ProductDescription,
                                     ProductCategorySimple = Products.ProductCategorySimple
                                 }).ToList();

            var total = 0;

            foreach(var item in ViewBag.CartItems)
            {
                total = total + (item.ProductPrice * item.ItemQuantity);
            }

            ViewBag.CartTotal = total;

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
            ViewBag.Colors = _repo.Products.Select(x => x.ProductPrimaryColor).Distinct().ToList();
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
            ViewBag.Colors = _repo.Products.Select(x => x.ProductPrimaryColor).Distinct().ToList();
            return View();
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public IActionResult AdminAddProduct(Product product) 
        {
            _repo.AddProduct(product);

            return RedirectToAction("AdminProducts");
        }
        
        [HttpPost]
        public void LogCspReport([FromBody] dynamic report)
        {
            // Log the CSP report to the console
            Console.WriteLine($"CSP Violation: {report}");
            // You might need to adjust the parsing of the 'report' object 
            // depending on the structure of your CSP violation reports
        }
        
        //[Authorize(Roles = "Admin")]
        //public IActionResult AdminUsers(int page = 1, int pageSize = 10)
        //{
        //    // Calculate the number of items to skip based on the page number and page size
        //    int skip = (page - 1) * pageSize;

        //    // ViewBag.Users = _repo.AspNetUsers
        //        // .SelectMany(u => u.Roles, (user, role) => new { user, role.Name })
        //        // .ToList();

        //    // Count the total number of customers
        //    // int totalUsers = _repo.AspNetUsers.Count();

        //    // Calculate the total number of pages
        //    // int totalPages = (int)Math.Ceiling((double)totalUsers / pageSize);

        //    // Pass the customers and pagination information to the view
        //    // ViewBag.TotalPages = totalPages;
        //    ViewBag.CurrentPage = page;

        //    return View();
        //}

        //[Authorize(Roles = "Admin")]
        //[HttpGet]
        //public IActionResult AdminDeleteUser(string userId)
        //{
        //    // var recordToDelete = _repo.AspNetUsers.Single(y => y.Id.Equals(userId));

        //    // return View(recordToDelete);
        //    return View();
        //}
        

        //[Authorize(Roles = "Admin")]
        //[HttpPost]
        //public IActionResult AdminDeleteUser(AspNetUser user)
        //{
        //    _repo.DeleteUser(user);

        //    return RedirectToAction("AdminUsers");
        //}

        //[Authorize(Roles = "Admin")]
        //[HttpGet]
        //public IActionResult AdminAddUser()
        //{
        //    return View();
        //}
        
        //[Authorize(Roles = "Admin")]
        //[HttpPost]
        //public IActionResult AdminAddUser(Customer customer)
        //{
        //    _repo.AddUser(customer);

        //    return RedirectToAction("AdminUsers");
        //}

        [Authorize(Roles = "Admin")]
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

        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> Dashboard()
        {
            var customer = HttpContext.User;

            var user = await _signInManager.UserManager.GetUserAsync(customer);

            var userName = user.UserName;

            ViewBag.CustomerDetails = _repo.Customers.Where(x => x.CustomerEmail.Equals(userName)).FirstOrDefault();

            return View();
        }

        [HttpPost]
        public IActionResult UpdateOrderStatus(Order order)
        {
            _repo.UpdateOrder(order);
            return RedirectToAction("AdminOrders");
        }

        [HttpGet]
        public async Task<IActionResult> SubmitOrder()
        {

            var customer = HttpContext.User;

            var user = await _signInManager.UserManager.GetUserAsync(customer);

            var userName = user.UserName;

            int customerId = _repo.Customers.Where(x => x.CustomerEmail.Equals(userName)).Select(x => x.CustomerId).FirstOrDefault();

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

            List<Cart> CartItems = _repo.Carts.Where(x => x.CustomerId.Equals(order.CustomerId)).ToList();

            foreach (var item in CartItems) 
            {
                LineItem lineItem = new LineItem
                {
                    TransactionId = addedOrder.TransactionId, 
                    ProductId = item.ProductId, 
                    Quantity = (int)item.ItemQuantity
                };

                _repo.AddLineItem(lineItem);

                _repo.RemoveCart(item);
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult AddCustomer(int customerId)
        {
            var recordToEdit = _repo.Customers.Single(x => x.CustomerId == customerId);

            return View(recordToEdit);
        }

        [HttpPost]
        public IActionResult AddCustomer(Customer customer)
        {
            _repo.EditCustomer(customer);

            return RedirectToAction("Index");
        }
    }
}
