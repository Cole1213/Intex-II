using System;
using System.Collections.Generic;

namespace Intex_II.Models;

public partial class LineItem
{
    public int TransactionId { get; set; }

    public int ProductId { get; set; }

    public int Quantity { get; set; }

    public int? Rating { get; set; }
}
