using System;
using System.Collections.Generic;

namespace Intex_II.Models;

public partial class Order
{
    public int TransactionId { get; set; }

    public int CustomerId { get; set; }

    public DateOnly TransactionDate { get; set; }

    public string? TransactionDayOfWeek { get; set; }

    public int? TransactionTime { get; set; }

    public string EntryMode { get; set; } = null!;

    public int Amount { get; set; }

    public string TransactionType { get; set; } = null!;

    public string TransactionCountry { get; set; } = null!;

    public string TransactionShippingAddress { get; set; } = null!;

    public string TransactionBank { get; set; } = null!;

    public string TransactionTypeOfCard { get; set; } = null!;

    public bool? Fraud { get; set; }

    public string? Status { get; set; }
}
