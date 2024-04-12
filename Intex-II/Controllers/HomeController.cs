using Azure;
using Intex_II.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Security.Cryptography.Xml;
using Microsoft.AspNetCore.Authorization;
using Microsoft.CodeAnalysis;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;

namespace Intex_II.Controllers
{
    public class HomeController : Controller
    {
        private ILegoRepository _repo;
        private SignInManager<IdentityUser> _signInManager;
        private InferenceSession _session;
        private UserManager<IdentityUser> _userManager;

        public HomeController(ILegoRepository temp, SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager)
        {
            _repo = temp;
            _signInManager = signInManager;
            _session = new InferenceSession("wwwroot/lib/Model/decision_tree_classifierWompWomp.onnx");
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {   
            string userName = null; // Initialize userId with null

            var customer = HttpContext.User;

            int customerId;
            if (customer.Identity.IsAuthenticated)
            {
                var user = await _signInManager.UserManager.GetUserAsync(customer);
                var userRoles = await _userManager.GetRolesAsync(user);

                if (userRoles.Contains("Admin"))
                {
                    return RedirectToAction("Index", "UserAdmin");
                }

                userName = user.UserName;

                customerId = _repo.Customers.Where(x => x.CustomerEmail.Equals(userName)).Select(x => x.CustomerId).FirstOrDefault();

                ViewBag.CartItemCount = _repo.Carts.Where(x => x.CustomerId.Equals(customerId)).Count();

                var userBased = _repo.UserBasedRecommendations.Where(x => x.CustomerId.Equals(customerId)).Select(x => x.ProductPurchased).Distinct().ToList();

                if (userBased is not null)
                {
                    List<int> myList = new List<int>();

                    foreach(var item in userBased)
                    {
                        myList.Add(item);
                    }

                    // Get the count of the list
                    int count = myList.Count;

                    // Create a Random object
                    Random random = new Random();

                    // Generate a random number between 1 and the count of the list
                    int randomNumber = random.Next(1, count + 1);

                    int productPurchasedId = myList[randomNumber];

                    ViewBag.BecauseYouPurchased = _repo.Products.Where(x => x.ProductId == productPurchasedId).ToList();

                    ViewBag.Recommendations = (from UserBasedRecommendations in _repo.UserBasedRecommendations
                                               join Products in _repo.Products
                                               on UserBasedRecommendations.RecommendedProductId equals Products.ProductId
                                               where UserBasedRecommendations.ProductPurchased == productPurchasedId
                                               orderby UserBasedRecommendations.Rank
                                               select new
                                               {
                                                   ProductId = Products.ProductId,
                                                   ProductName = Products.ProductName,
                                                   ProductDescription = Products.ProductDescription,
                                                   ProductPrice = Products.ProductPrice,
                                                   ProductCategory = Products.ProductCategory,
                                                   ProductImage = Products.ProductImage,
                                                   ProductCategorySimple = Products.ProductCategorySimple,
                                                   Rank = UserBasedRecommendations.Rank
                                               }).ToList();
                }
                else
                {
                    ViewBag.Recommendations = _repo.Products.Take(5).ToList();
                }
            }
            else
            {
                ViewBag.Recommendations = _repo.Products.Take(5).ToList();
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
        public async Task<IActionResult> Products(List<string> categories = null, List<string> Colors = null, decimal? minPrice = null, decimal? maxPrice = null, int page = 1, int itemsPerPage = 10, string addedToCart = null)
        {
            string userName = null; // Initialize userId with null

            var customer = HttpContext.User;

            int customerId;
            if (customer.Identity.IsAuthenticated)
            {
                var user = await _signInManager.UserManager.GetUserAsync(customer);

                userName = user.UserName;

                customerId = _repo.Customers.Where(x => x.CustomerEmail.Equals(userName)).Select(x => x.CustomerId).FirstOrDefault();

                ViewBag.CartItemCount = _repo.Carts.Where(x => x.CustomerId.Equals(customerId)).Count();
            }

            ViewBag.CustomerId = _repo.Customers.Where(x => x.CustomerEmail.Equals(userName)).Select(x => x.CustomerId).FirstOrDefault();

            IQueryable<Product> productsQuery = _repo.Products; // Assuming _repo.Products is IQueryable<Product>

            // Apply filters based on categories and price if provided
            if (categories != null && categories.Any())
            {
                // Filter products that belong to any of the selected categories
                productsQuery = productsQuery.Where(p => categories.Contains(p.ProductCategorySimple));
            }

            if (Colors != null && Colors.Any())
            {
                productsQuery = productsQuery.Where(p => Colors.Contains(p.ProductPrimaryColor));
            }


            if (minPrice.HasValue)
            {
                productsQuery = productsQuery.Where(p => p.ProductPrice >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                productsQuery = productsQuery.Where(p => p.ProductPrice <= maxPrice.Value);
            }

            int skip = (page - 1) * itemsPerPage;

            var filteredProducts = productsQuery.ToList();
            // Retrieve a specific page of products
            var pageProducts = productsQuery.Skip(skip).Take(itemsPerPage).ToList();

            int totalPages = (int)Math.Ceiling((double)filteredProducts.Count / itemsPerPage);

            // Pass pagination information to the view
            ViewBag.TotalPages = totalPages;
            ViewBag.ItemsPerPage = itemsPerPage;
            ViewBag.Page = page;

            ViewBag.Products = pageProducts;

            // Pass in categories for the checkbox filters
            ViewBag.Categories = _repo.Products
                                    .Select(p => p.ProductCategorySimple)
                                    .Distinct()
                                    .ToList();

            // Pass in colors for the checkbox filters
            ViewBag.Colors = _repo.Products
                                 .Select(p => p.ProductPrimaryColor)
                                 .Distinct()
                                 .ToList();

            ViewBag.AddedToCart = addedToCart;

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

            return RedirectToAction("Products", new { addedToCart = "Professor Hilton is the GOAT" });
        }

        [HttpGet]
        public async Task<IActionResult> SingleProduct(int productId)
        {
            string userName = null; // Initialize userId with null

            var customer = HttpContext.User;

            int customerId;
            if (customer.Identity.IsAuthenticated)
            {
                var user = await _signInManager.UserManager.GetUserAsync(customer);

                userName = user.UserName;

                customerId = _repo.Customers.Where(x => x.CustomerEmail.Equals(userName)).Select(x => x.CustomerId).FirstOrDefault();

                ViewBag.CartItemCount = _repo.Carts.Where(x => x.CustomerId.Equals(customerId)).Count();
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

        [Authorize(Roles = "Admin")]
        public IActionResult AdminOrders(int page = 1, int itemsPerPage = 50)
        {
            var orders = _repo.Orders.OrderByDescending(x => x.TransactionDate).Take(500).ToList();

            int skip = (page - 1) * itemsPerPage;

            // Retrieve a specific page of products
            var pageProducts = orders.Skip(skip).Take(itemsPerPage).ToList();

            int totalPages = (int)Math.Ceiling((double)orders.Count / itemsPerPage);

            ViewBag.Orders = pageProducts;
            // Pass pagination information to the view
            ViewBag.TotalPages = totalPages;
            ViewBag.ItemsPerPage = itemsPerPage;
            ViewBag.Page = page;

            ViewBag.Products = pageProducts;

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

            ViewBag.Countries = _repo.Orders
                                    .Select(p => p.TransactionCountry)
                                    .Distinct()
                                    .ToList();

            ViewBag.Banks = _repo.Orders
                                    .Select(p => p.TransactionBank)
                                    .Distinct()
                                    .ToList();

            var total = 0;

            foreach (var item in ViewBag.CartItems)
            {
                total = total + (item.ProductPrice * item.ItemQuantity);
            }

            ViewBag.CartTotal = total;

            return View();
        }
        
        [HttpPost]
        public IActionResult SubmitOrder(Order order) 
        {
            var customerId = order.CustomerId;
            var time = order.TransactionTime;
            var orderAmount = order.Amount;

            var day_of_week_Mon = 0;
            var day_of_week_Tue = 0;
            var day_of_week_Wed = 0;
            var day_of_week_Thu = 0;
            var day_of_week_Sat = 0;
            var day_of_week_Sun = 0;
            if (order.TransactionDayOfWeek.Equals("Mon"))
            {
                day_of_week_Mon = 1;
            }
            if (order.TransactionDayOfWeek.Equals("Tue"))
            {
                day_of_week_Tue = 1;
            }
            if (order.TransactionDayOfWeek.Equals("Wed"))
            {
                day_of_week_Wed = 1;
            }
            if (order.TransactionDayOfWeek.Equals("Thu"))
            {
                day_of_week_Thu = 1;
            }
            if (order.TransactionDayOfWeek.Equals("Sat"))
            {
                day_of_week_Sat = 1;
            }
            if (order.TransactionDayOfWeek.Equals("Sun"))
            {
                day_of_week_Sun = 1;
            }

            var entry_mode_PIN = 0;
            var entry_mode_Tap = 0;

            var type_of_transaction_Online = 1;
            var type_of_transaction_POS = 0;

            var country_of_transaction_India = 0;
            var country_of_transaction_Russia = 0;
            var country_of_transaction_USA = 0;
            var country_of_transaction_United_Kingdom = 0;
            var shipping_address_India = 0;
            var shipping_address_Russia = 0;
            var shipping_address_USA = 0;
            var shipping_address_United_Kingdom = 0;
            if (order.TransactionCountry.Equals("India"))
            {
                country_of_transaction_India = 1;
                shipping_address_India = 1;
            }
            if (order.TransactionCountry.Equals("Russia"))
            {
                country_of_transaction_Russia = 1;
                shipping_address_Russia = 1;
            }
            if (order.TransactionCountry.Equals("USA"))
            {
                country_of_transaction_USA = 1;
                shipping_address_USA = 1;
            }
            if (order.TransactionCountry.Equals("United Kingdom"))
            {
                country_of_transaction_United_Kingdom = 1;
                shipping_address_United_Kingdom = 1;
            }

            var bank_HSBC = 0;
            var bank_Halifax = 0;
            var bank_Lloyds = 0;
            var bank_Metro = 0;
            var bank_Monzo = 0;
            var bank_RBS = 0;
            if (order.TransactionBank.Equals("HSBC"))
            {
                bank_HSBC = 1;
            }
            if (order.TransactionBank.Equals("Halifax"))
            {
                bank_Halifax = 1;
            }
            if (order.TransactionBank.Equals("Lloyds"))
            {
                bank_Lloyds = 1;
            }
            if (order.TransactionBank.Equals("Metro"))
            {
                bank_Metro = 1;
            }
            if (order.TransactionBank.Equals("Monzo"))
            {
                bank_Monzo = 1;
            }
            if (order.TransactionBank.Equals("RBS"))
            {
                bank_RBS = 1;
            }

            var type_of_card_Visa = 0;
            if (order.TransactionTypeOfCard.Equals("Visa"))
            {
                type_of_card_Visa = 1;
            }

            var input = new List<float> { customerId, time, (float)orderAmount, day_of_week_Mon, day_of_week_Sat, day_of_week_Sun, day_of_week_Thu, day_of_week_Tue, day_of_week_Wed, entry_mode_PIN, entry_mode_Tap, type_of_transaction_Online, type_of_transaction_POS, country_of_transaction_India, country_of_transaction_Russia, country_of_transaction_USA, country_of_transaction_United_Kingdom, shipping_address_India, shipping_address_Russia, shipping_address_USA, shipping_address_United_Kingdom, bank_HSBC, bank_Halifax, bank_Lloyds, bank_Metro, bank_Monzo, bank_RBS, type_of_card_Visa };
            var inputTensor = new DenseTensor<float>(input.ToArray(), new[] { 1, input.Count });

            var inputs = new List<NamedOnnxValue>
            {
                NamedOnnxValue.CreateFromTensor("float_input", inputTensor)
            };

            int prediction;
            using (var results = _session.Run(inputs))
            {
                var predictionTensor = (DenseTensor<long>)results[0].Value;
                prediction = (int)predictionTensor.GetValue(0);
            }

            bool isFraud;
            if (prediction == 0)
            {
                isFraud = false;
            }
            else
            {
                isFraud = true;
            }

            order.Fraud = isFraud;
            order.Status = "Pending";

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

            return RedirectToAction("OrderConfirm", new { orderId = addedOrder.TransactionId });
        }

        public IActionResult OrderConfirm(int orderId)
        {
            ViewBag.Order = _repo.Orders.Where(x => x.TransactionId == orderId).FirstOrDefault();

            return View();
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
