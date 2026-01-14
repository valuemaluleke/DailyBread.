using Microsoft.EntityFrameworkCore;
using DailyBread.Models;

namespace DailyBread.Data
{
    public class DailyBreadContext : DbContext
    {
        public DailyBreadContext(DbContextOptions<DailyBreadContext> options)
            : base(options)
        {
        }

        // Table 1: Your Saved Verses
        public DbSet<FavoriteVerse> Favorites { get; set; }
        
        // 👇 Table 2: Your Prayer Requests (MAKE SURE THIS IS HERE!)
        public DbSet<PrayerRequest> PrayerRequests { get; set; }
    }
}