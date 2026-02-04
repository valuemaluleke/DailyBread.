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

        public DbSet<FavoriteVerse> Favorites { get; set; }
        
        public DbSet<PrayerRequest> PrayerRequests { get; set; }
    }
}
