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

        //Get for the Idex page
        [HttpGet]
        public async Task<IActionResult> Index()
        {   
            string userName = null; // Initialize userId with null

            var customer = HttpContext.User; // Get the current user info

            int customerId; // Initialize
            if (customer.Identity.IsAuthenticated) // If user is authenticated
            {
                //get user info
                var user = await _signInManager.UserManager.GetUserAsync(customer);
                var userRoles = await _userManager.GetRolesAsync(user);

                //If user is an admin, send them to a different index page
                if (userRoles.Contains("Admin"))
                {
                    return RedirectToAction("Index", "UserAdmin");
                }

                //Get the username and customerId for the current user
                userName = user.UserName;

                customerId = _repo.Customers.Where(x => x.CustomerEmail.Equals(userName)).Select(x => x.CustomerId).FirstOrDefault();

                //Get the count of items in the cart for this user
                ViewBag.CartItemCount = _repo.Carts.Where(x => x.CustomerId.Equals(customerId)).Count();

                //Get our UserBased recommendations for the user with Transaction history
                var userBased = _repo.UserBasedRecommendations.Where(x => x.CustomerId.Equals(customerId)).Select(x => x.ProductPurchased).Distinct().ToList();

                if (userBased.Count() != 0)
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
                    int randomNumber = random.Next(0, count);

                    // Get the ProductId for the item in their transaction history
                    int productPurchasedId = myList[randomNumber];

                    // Assign a variable for the item they purchased
                    ViewBag.BecauseYouPurchased = _repo.Products.Where(x => x.ProductId == productPurchasedId).FirstOrDefault();

                    // Get recommendations based on the purchased item
                    var recommendations = (from userBasedRecommendations in _repo.UserBasedRecommendations
                                           join products in _repo.Products
                                           on userBasedRecommendations.RecommendedProductId equals products.ProductId
                                           where userBasedRecommendations.ProductPurchased == productPurchasedId
                                           select new
                                           {
                                               ProductId = products.ProductId,
                                               ProductName = products.ProductName,
                                               ProductDescription = products.ProductDescription,
                                               ProductPrice = products.ProductPrice,
                                               ProductCategory = products.ProductCategory,
                                               ProductImage = products.ProductImage,
                                               ProductCategorySimple = products.ProductCategorySimple,
                                               Rank = userBasedRecommendations.Rank
                                           }).ToList();

                    var randomShuffle = new Random();
                    var shuffledRecommendations = recommendations.OrderBy(x => randomShuffle.Next()).ToList();

                    ViewBag.Recommendations = shuffledRecommendations;
                }
                else
                {
                    //If they don't have User-Based recommendations, then present them with top products
                    ViewBag.Recommendations = (from TopProducts in _repo.TopProducts
                                               join products in _repo.Products
                                               on TopProducts.ProductId equals products.ProductId
                                               orderby TopProducts.Rating descending
                                               select new
                                               {
                                                   ProductId = products.ProductId,
                                                   ProductName = products.ProductName,
                                                   ProductDescription = products.ProductDescription,
                                                   ProductPrice = products.ProductPrice,
                                                   ProductCategory = products.ProductCategory,
                                                   ProductImage = products.ProductImage,
                                                   ProductCategorySimple = products.ProductCategorySimple,
                                               }).ToList();
                }
            }
            else
            {
                //If they don't have User-Based recommendations, then present them with top products
                ViewBag.Recommendations = (from TopProducts in _repo.TopProducts
                                           join products in _repo.Products
                                           on TopProducts.ProductId equals products.ProductId
                                           orderby TopProducts.Rating descending
                                           select new
                                           {
                                               ProductId = products.ProductId,
                                               ProductName = products.ProductName,
                                               ProductDescription = products.ProductDescription,
                                               ProductPrice = products.ProductPrice,
                                               ProductCategory = products.ProductCategory,
                                               ProductImage = products.ProductImage,
                                               ProductCategorySimple = products.ProductCategorySimple,
                                           }).ToList();
            }

            ViewBag.CustomerId = _repo.Customers.Where(x => x.CustomerEmail.Equals(userName)).Select(x => x.CustomerId).FirstOrDefault();

            return View();
        }

        // Post action to add items to cart from the Index page
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

                _repo.AddCart(newCartItem);
            }
            else
            {
                // If the product doesn't exist in the cart, add a new entry
                _repo.AddCart(cart);
            }

            return RedirectToAction("Index");
        }

        // Get the products page
        [HttpGet]
        public IActionResult Community()
        {
            return View();
        }
        
        [HttpGet]
        public IActionResult Newsletter()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Products(List<string> categories = null, List<string> Colors = null, decimal? minPrice = null, decimal? maxPrice = null, int page = 1, int itemsPerPage = 10, string addedToCart = null)
        {
            string userName = null; // Initialize userId with null

            // Get customer info
            var customer = HttpContext.User;

            int customerId;
            if (customer.Identity.IsAuthenticated)
            {
                var user = await _signInManager.UserManager.GetUserAsync(customer);

                userName = user.UserName;

                customerId = _repo.Customers.Where(x => x.CustomerEmail.Equals(userName)).Select(x => x.CustomerId).FirstOrDefault();

                //Pass in the count of products in a cart for this customer
                ViewBag.CartItemCount = _repo.Carts.Where(x => x.CustomerId.Equals(customerId)).Count();
            }

            ViewBag.CustomerId = _repo.Customers.Where(x => x.CustomerEmail.Equals(userName)).Select(x => x.CustomerId).FirstOrDefault();

            IQueryable<Product> productsQuery = _repo.Products; // Assuming _repo.Products is IQueryable<Product>

            // Apply filters based on categories
            if (categories != null && categories.Any())
            {
                // Filter products that belong to any of the selected categories
                productsQuery = productsQuery.Where(p => categories.Contains(p.ProductCategorySimple));
            }
            // Apply filters based on Colors
            if (Colors != null && Colors.Any())
            {
                productsQuery = productsQuery.Where(p => Colors.Contains(p.ProductPrimaryColor));
            }

            // Apply filters based on price
            if (minPrice.HasValue)
            {
                productsQuery = productsQuery.Where(p => p.ProductPrice >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                productsQuery = productsQuery.Where(p => p.ProductPrice <= maxPrice.Value);
            }

            //Pagination stuff
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

        // Post action for adding items to cart from the Products page
        [HttpPost]
        public IActionResult Products(Cart cart)
        {
            //Check for existing items in car
            var existingCartItem = _repo.Carts
                .FirstOrDefault(c => c.CustomerId == cart.CustomerId && c.ProductId == cart.ProductId);

            //If there is an existing item
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

        // Get the product details page
        [HttpGet]
        public async Task<IActionResult> SingleProduct(int productId)
        {
            //Get customer info
            string userName = null; // Initialize userId with null

            var customer = HttpContext.User;

            int customerId;
            if (customer.Identity.IsAuthenticated)
            {
                var user = await _signInManager.UserManager.GetUserAsync(customer);

                userName = user.UserName;

                customerId = _repo.Customers.Where(x => x.CustomerEmail.Equals(userName)).Select(x => x.CustomerId).FirstOrDefault();

                //Pass in number of products in cart
                ViewBag.CartItemCount = _repo.Carts.Where(x => x.CustomerId.Equals(customerId)).Count();
            }

            ViewBag.CustomerId = _repo.Customers.Where(x => x.CustomerEmail.Equals(userName)).Select(x => x.CustomerId).FirstOrDefault();

            // Pass in the product details
            ViewBag.Products = _repo.Products.FirstOrDefault(p => p.ProductId == productId);

            // Pass in Similar products based on 
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
        
        // Post action to add to cart from this page
        [Authorize(Roles = "Customer")]
        [HttpPost]
        public IActionResult SingleProduct(Cart cart)
        {
            // Check for existing item
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
        
        // Get the privacy page
        public IActionResult Privacy()
        {
            return View();
        }

        // Get the About page
        public IActionResult About()
        {
            return View();
        }

        // Get the cart page
        [Authorize(Roles = "Customer")]
        [HttpGet]
        public async Task<IActionResult> Cart()
        {
            //User info
            var signedInUser = HttpContext.User;
            var user = await _signInManager.UserManager.GetUserAsync(signedInUser);
            var userName = user.UserName;
            int customerId = _repo.Customers.Where(x => x.CustomerEmail.Equals(userName)).Select(x => x.CustomerId).FirstOrDefault();
            ViewBag.CustomerId = customerId;

            //Pass in cart items
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

            //Get totoal cart price
            var total = 0;
            foreach(var item in ViewBag.CartItems)
            {
                total = total + (item.ProductPrice * item.ItemQuantity);
            }
            ViewBag.CartTotal = total;

            return View();
        }

        // Post action to allow users to remove items from cart
        [Authorize(Roles = "Customer")]
        [HttpPost]
        public IActionResult Cart(Cart cart)
        {
            _repo.RemoveCart(cart);

            return RedirectToAction("Cart");
        }

        // Get page where admin can access/edit/delete products
        [Authorize(Roles = "Admin")]
        public IActionResult AdminProducts()
        {
            ViewBag.Products = _repo.Products.OrderBy(x => x.ProductName).ToList();

            return View();
        }

        // Render the Edit Products page
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult AdminEditProduct(int productId)
        {
            ViewBag.Colors = _repo.Products.Select(x => x.ProductPrimaryColor).Distinct().ToList();
            var recordToEdit = _repo.Products.Single(x => x.ProductId == productId);

            return View("AdminAddProduct", recordToEdit);
        }
        
        // Update product
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public IActionResult AdminEditProduct(Product product)
        {
            _repo.UpdateProduct(product);

            return RedirectToAction("AdminProducts");
        } 
        
        // Get the deletion confirmation page
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult AdminDeleteProduct(int productId)
        {
            var recordToDelete = _repo.Products.Single(y => y.ProductId == productId);

            return View(recordToDelete);
        }
        
        // Acutally delete the specified product
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public IActionResult AdminDeleteProduct(Product product)
        {
            _repo.DeleteProduct(product);

            return RedirectToAction("AdminProducts");
        }
        
        // Render the Add Product page
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult AdminAddProduct()
        {
            ViewBag.Colors = _repo.Products.Select(x => x.ProductPrimaryColor).Distinct().ToList();
            return View();
        }

        // Submit a product to be added to the db
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

        // Get page where admin can view/edit orders
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

        // Get the User Information page
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> Dashboard()
        {
            // Get user info
            var customer = HttpContext.User;
            var user = await _signInManager.UserManager.GetUserAsync(customer);
            var userName = user.UserName;

            ViewBag.CustomerDetails = _repo.Customers.Where(x => x.CustomerEmail.Equals(userName)).FirstOrDefault();

            return View();
        }

        // Allow an Admin to update orders
        [HttpPost]
        public IActionResult UpdateOrderStatus(Order order)
        {
            _repo.UpdateOrder(order);
            return RedirectToAction("AdminOrders");
        }

        // Get the checkout page
        [HttpGet]
        public async Task<IActionResult> SubmitOrder()
        {
            // Get customer info
            var customer = HttpContext.User;
            var user = await _signInManager.UserManager.GetUserAsync(customer);
            var userName = user.UserName;
            int customerId = _repo.Customers.Where(x => x.CustomerEmail.Equals(userName)).Select(x => x.CustomerId).FirstOrDefault();

            //Pass in the items in the cart for this user
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

            // All of these are for dropdown menus
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

            // Get the cart total
            var total = 0;
            foreach (var item in ViewBag.CartItems)
            {
                total = total + (item.ProductPrice * item.ItemQuantity);
            }
            ViewBag.CartTotal = total;

            return View();
        }
        
        // Submit the order to the database and check if it may be fraudulent
        [HttpPost]
        public IActionResult SubmitOrder(Order order) 
        {
            // Set variables to pass into ML pipeline
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

            // Put inputs into a list
            var input = new List<float> { customerId, time, (float)orderAmount, day_of_week_Mon, day_of_week_Sat, day_of_week_Sun, day_of_week_Thu, day_of_week_Tue, day_of_week_Wed, entry_mode_PIN, entry_mode_Tap, type_of_transaction_Online, type_of_transaction_POS, country_of_transaction_India, country_of_transaction_Russia, country_of_transaction_USA, country_of_transaction_United_Kingdom, shipping_address_India, shipping_address_Russia, shipping_address_USA, shipping_address_United_Kingdom, bank_HSBC, bank_Halifax, bank_Lloyds, bank_Metro, bank_Monzo, bank_RBS, type_of_card_Visa };
            var inputTensor = new DenseTensor<float>(input.ToArray(), new[] { 1, input.Count });

            var inputs = new List<NamedOnnxValue>
            {
                NamedOnnxValue.CreateFromTensor("float_input", inputTensor)
            };

            // Generate prediction based on pipeline and user input
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

            // Add these details into the order before sending to the database
            order.Fraud = isFraud;
            order.Status = "Pending";

            // Add order to the database
            Order addedOrder = _repo.AddOrder(order);

            List<Cart> CartItems = _repo.Carts.Where(x => x.CustomerId.Equals(order.CustomerId)).ToList();

            // For each item in the new order, add new stuff to the lineItems table and remove the items from the cart
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

        // Get the order confirmation page
        public IActionResult OrderConfirm(int orderId)
        {
            ViewBag.Order = _repo.Orders.Where(x => x.TransactionId == orderId).FirstOrDefault();

            return View();
        }

        // Get the page to add Customer info after a new User is registered
        [HttpGet]
        public IActionResult AddCustomer(int customerId)
        {
            var recordToEdit = _repo.Customers.Single(x => x.CustomerId == customerId);

            return View(recordToEdit);
        }

        // Add the Customer data to the database
        [HttpPost]
        public IActionResult AddCustomer(Customer customer)
        {
            _repo.EditCustomer(customer);

            return RedirectToAction("Index");
        }
    }
}
