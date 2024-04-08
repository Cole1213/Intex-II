namespace Intex_II.Models
{
    public interface ILegoRepository
    {
        public IQueryable<Customer> Customers { get; }

        public IQueryable<Product> Products { get; }

        public IQueryable<Order> Orders { get; }

        public IQueryable<LineItem> LineItems { get; }

        public void UpdateUser(Customer customer);

        public void AddUser(Customer customer);

        public void UpdateProduct(Product product);

        public void AddProduct(Product product);

        public void UpdateOrder(Order order);

        public void AddOrder(Order order);

    }
}
