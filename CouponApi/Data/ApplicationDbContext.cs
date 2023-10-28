using CouponApi.Models;
using Microsoft.EntityFrameworkCore;

namespace CouponApi.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            :base(options)
        {
            
        }
        public DbSet<Coupon> Coupons { get; set; }
    }
}
