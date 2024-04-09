namespace Intex_II.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

public class IdentityDataContext : IdentityDbContext
{
    public IdentityDataContext(DbContextOptions<IdentityDataContext> options)
        : base(options)
    {
    }

    public IdentityDataContext(DbContextOptions options) : base(options)
    {
        
    }

    // If you have additional DbSets for other entities, define them here
}