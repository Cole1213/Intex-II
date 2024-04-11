namespace Intex_II.Models
{
    public interface ILegoRepository
    {
        public IQueryable<Customer> Customers { get; }

        public IQueryable<Product> Products { get; }

        public IQueryable<Order> Orders { get; }

        public IQueryable<LineItem> LineItems { get; }

        public IQueryable<Cart> Carts { get; }

        // public IQueryable<AspNetUser> AspNetUsers { get; }

        public void AddCart(Cart cart);

        public void RemoveCart(Cart cart);

        public void UpdateUser(Customer customer);

        public void DeleteUser(AspNetUser user);

        public void AddUser(Customer customer);

        public void UpdateProduct(Product product);

        public void AddProduct(Product product);

        public void DeleteProduct(Product product);

        public void UpdateOrder(Order order);

        public Order AddOrder(Order order);

        public void AddLineItem(LineItem lineItem);

        public Customer AddCustomer(Customer customer);

        public void EditCustomer(Customer customer);

    }
}
