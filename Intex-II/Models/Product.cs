using System;
using System.Collections.Generic;

namespace Intex_II.Models;

public partial class Product
{
    public string ProductName { get; set; } = null!;

    public int? ProductYear { get; set; }

    public int? ProductNumParts { get; set; }

    public int ProductPrice { get; set; }

    public string? ProductImage { get; set; }

    public string? ProductPrimaryColor { get; set; }

    public string? ProductSecondaryColor { get; set; }

    public string ProductDescription { get; set; } = null!;

    public string? ProductCategory { get; set; }

    public string? ProductCategorySimple { get; set; }

    public int ProductId { get; set; }
}
