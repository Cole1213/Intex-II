using System.ComponentModel.DataAnnotations;

namespace Intex_II.Models
{
    public class Customer
    {
        [Key]
        public int Customer_Id { get; set; }

        public string? Customer_FName { get; set; }

        public string? Customer_LName { get; set;}

        public DateOnly? Birth_Date { get; set; }

        public string? Customer_Country { get; set; }

        public char? Customer_Gender { get; set; }

        public double? Customer_Age { get; set; }
    }
}
