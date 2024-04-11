using System;
using System.Collections.Generic;

namespace Intex_II.Models;

public partial class Cart
{
    public int CustomerId { get; set; }

    public int ProductId { get; set; }

    public int? ItemQuantity { get; set; }

    public int? TotalPrice { get; set; }
}
