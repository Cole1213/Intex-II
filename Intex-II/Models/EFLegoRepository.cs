namespace Intex_II.Models
{
    public class EFLegoRepository : ILegoRepository
    {
        private IntexIiContext _context;
        public EFLegoRepository(IntexIiContext temp)
        {
            _context = temp;
        }
        public IQueryable<Customer> Customers => _context.Customers;

        public IQueryable<Product> Products => _context.Products;

        public IQueryable<Order> Orders => _context.Orders;

        public IQueryable<LineItem> LineItems => _context.LineItems;

        public void UpdateUser(Customer customer)
        {
            _context.Customers.Update(customer);
            _context.SaveChanges();
        }

        public void AddUser(Customer customer)
        {
            _context.Customers.Add(customer);
            _context.SaveChanges();
        }

        public void UpdateProduct(Product product)
        {
            _context.Products.Update(product);
            _context.SaveChanges();
        }

        public void AddProduct(Product product)
        {
            _context.Products.Add(product);
            _context.SaveChanges();
        }

        public void UpdateOrder(Order order)
        {
            _context.Orders.Update(order);
            _context.SaveChanges();
        }

        public void AddOrder(Order order)
        {
            _context.Orders.Add(order);
            _context.SaveChanges();
        }
    }
}
