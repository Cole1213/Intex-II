using System.ComponentModel.DataAnnotations.Schema;

namespace Intex_II.Models
{
    public class LineItem
    {
        [ForeignKey("Transaction_Id")]
        public int Transaction_Id { get; set; }

        [ForeignKey("Product_Id")]
        public int Product_Id { get; set; }

        public int Quantity { get; set; }

        public int? Rating { get; set; }
    }
}
