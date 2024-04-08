using System.ComponentModel.DataAnnotations;

namespace Intex_II.Models
{
    public class Product
    {
        [Key]
        public int Product_Id { get; set; }

        public string Product_Name { get; set; }

        public int? Product_Year { get; set; }

        public int? Product_Num_Parts { get; set; }

        public double Product_Price { get; set; }

        public string? Product_Image { get; set; }

        public string? Product_Primary_Color { get; set; }

        public string? Product_Secondary_Color { get; set; }

        public string? Product_Description { get; set;}

        public string? Product_Category { get; set; }
    }
}
