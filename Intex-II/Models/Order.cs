using System;
using System.Collections.Generic;

namespace Intex_II.Models;

public partial class Order
{
    public int CustomerId { get; set; }

    public DateOnly TransactionDate { get; set; }

    public string TransactionDayOfWeek { get; set; } = null!;

    public int TransactionTime { get; set; }

    public string? EntryMode { get; set; }

    public int? Amount { get; set; }

    public string? TransactionType { get; set; }

    public string? TransactionCountry { get; set; }

    public string? TransactionShippingAddress { get; set; }

    public string? TransactionBank { get; set; }

    public string? TransactionTypeOfCard { get; set; }

    public bool? Fraud { get; set; }

    public string? Status { get; set; }

    public int TransactionId { get; set; }
}
