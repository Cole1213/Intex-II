using System;
using System.Collections.Generic;

namespace Intex_II.Models;

public partial class UserBasedRecommendation
{
    public int CustomerId { get; set; }

    public int ProductPurchased { get; set; }

    public int RecommendedProductId { get; set; }

    public int Rank { get; set; }
}
