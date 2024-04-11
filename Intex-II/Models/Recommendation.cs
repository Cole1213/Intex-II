using System;
using System.Collections.Generic;

namespace Intex_II.Models;

public partial class Recommendation
{
    public byte ProductId { get; set; }

    public byte RecommendedProductId { get; set; }

    public byte Rank { get; set; }
}
