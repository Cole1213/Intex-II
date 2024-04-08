using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Intex_II.Models
{
    public class Order
    {
        [Key]
        public int Transaction_Id { get; set; }

        [ForeignKey("Customer_Id")]
        public int Customer_Id { get; set; }

        public DateOnly Transaction_Date { get; set; }

        public string? Transaction_Day_Of_Week { get; set; }

        public int? Transaction_Time { get; set; }

        public string Entry_Mode { get; set; }

        public int Amount {  get; set; }

        public string Transaction_Type { get; set; }

        public string Transaction_Country { get; set; }

        public string Transaction_Shipping_Address { get; set; }

        public string Transaction_Bank { get; set; }

        public string Transaction_Type_Of_Card { get; set; }

        public bool? Fraud { get; set; }
    }
}
