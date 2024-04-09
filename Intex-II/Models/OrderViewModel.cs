namespace Intex_II.Models
{
    public class OrderViewModel
    {
        public Order Order { get; set; }
        public List<LineItem> LineItems { get; set; }

        public List<Cart> Carts { get; set; }
    }
}
