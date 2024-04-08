using System;
using System.Collections.Generic;

namespace Intex_II.Models;

public partial class Customer
{
    public int CustomerId { get; set; }

    public string? CustomerFname { get; set; }

    public string? CustomerLname { get; set; }

    public DateOnly? BirthDate { get; set; }

    public string? CustomerCountry { get; set; }

    public string? CustomerGender { get; set; }

    public double? CustomerAge { get; set; }
}
