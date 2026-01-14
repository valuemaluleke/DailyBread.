using DailyBread.Models;
using DailyBread.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Text.Json;

namespace DailyBread.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly HttpClient _httpClient;
        private readonly DailyBreadContext _context;

        // --- EXISTING MAPS ---
        private readonly Dictionary<string, List<string>> _moodMap = new()
        {
            { "anxious", new List<string> { "Philippians 4:6-7", "1 Peter 5:7", "Psalm 23:4", "Matthew 6:34", "Isaiah 41:10", "John 14:27" } },
            { "sad", new List<string> { "Psalm 34:18", "Revelation 21:4", "Matthew 5:4", "Psalm 147:3", "2 Corinthians 1:3-4", "Psalm 73:26" } },
            { "grateful", new List<string> { "1 Thessalonians 5:18", "Psalm 107:1", "James 1:17", "Colossians 3:17", "Psalm 118:24", "Psalm 100:4" } },
            { "hopeful", new List<string> { "Jeremiah 29:11", "Romans 15:13", "Isaiah 40:31", "Hebrews 11:1", "Lamentations 3:22-23" } }
        };

        private readonly Dictionary<string, string> _prayerMap = new()
        {
            { "anxious", "Lord, my heart is heavy with worry today. Help me to cast all my anxieties on You, knowing that You care for me. Grant me Your peace that passes all understanding. Amen." },
            { "sad", "Father, I feel brokenhearted today. Please wrap Your loving arms around me and remind me that You are close to those who are hurting. Wipe away my tears and bring comfort to my soul. Amen." },
            { "grateful", "Lord, thank You for the blessings You have poured into my life! My heart is full of praise. Help me to never take Your grace for granted and to share this joy with others. Amen." },
            { "hopeful", "God, I trust in Your plans for me. Thank You for giving me a future and a hope. Strengthen my faith as I walk forward, knowing You are already there. Amen." },
            { "default", "Lord, thank You for Your Word today. Help me to hide it in my heart so that I might not sin against You. Guide my steps and let Your wisdom lead me. Amen." }
        };

        // Reading Plans
        private static readonly List<ReadingPlan> _plans = new()
        {
            new ReadingPlan 
            { 
                Id = "peace", 
                Title = "7 Days of Peace", 
                Description = "Find rest for your anxious soul in God's presence.", 
                ImageEmoji = "🕊️",
                Days = new List<PlanDay>
                {
                    new PlanDay { DayNumber = 1, Reference = "John 14:27", Theme = "Peace I leave with you" },
                    new PlanDay { DayNumber = 2, Reference = "Isaiah 26:3", Theme = "Perfect Peace" },
                    new PlanDay { DayNumber = 3, Reference = "Philippians 4:7", Theme = "Guarding your heart" },
                    new PlanDay { DayNumber = 4, Reference = "Psalm 4:8", Theme = "Sleeping in safety" },
                    new PlanDay { DayNumber = 5, Reference = "Matthew 11:28", Theme = "Rest for the weary" },
                    new PlanDay { DayNumber = 6, Reference = "2 Thessalonians 3:16", Theme = "Lord of Peace" },
                    new PlanDay { DayNumber = 7, Reference = "Numbers 6:24-26", Theme = "The Blessing" }
                }
            },
            new ReadingPlan 
            { 
                Id = "trust", 
                Title = "Trusting God", 
                Description = "Learning to let go and let God lead the way.", 
                ImageEmoji = "⚓",
                Days = new List<PlanDay>
                {
                    new PlanDay { DayNumber = 1, Reference = "Proverbs 3:5-6", Theme = "Trust in the Lord" },
                    new PlanDay { DayNumber = 2, Reference = "Psalm 28:7", Theme = "My Strength and Shield" },
                    new PlanDay { DayNumber = 3, Reference = "Isaiah 12:2", Theme = "I will not be afraid" },
                    new PlanDay { DayNumber = 4, Reference = "Psalm 56:3", Theme = "When I am afraid" },
                    new PlanDay { DayNumber = 5, Reference = "Jeremiah 17:7", Theme = "Blessed is the one" }
                }
            }
        };

        public HomeController(ILogger<HomeController> logger, DailyBreadContext context)
        {
            _logger = logger;
            _httpClient = new HttpClient();
            _context = context;
        }

        public async Task<IActionResult> Index(string mood, string userFeeling, string translation)
        {
            UpdateStreak(); // 🔥 This now checks for missed days too!

            if (!string.IsNullOrEmpty(translation))
                Response.Cookies.Append("BibleVersion", translation, new CookieOptions { Expires = DateTime.Now.AddYears(1) });
            else
                translation = Request.Cookies["BibleVersion"] ?? "web";
            
            ViewBag.CurrentVersion = translation;

            string bibleApiUrl;
            string selectedPrayer;

            if (!string.IsNullOrEmpty(mood) && _moodMap.ContainsKey(mood.ToLower()))
            {
                var verseList = _moodMap[mood.ToLower()];
                var random = new Random();
                var selectedRef = verseList[random.Next(verseList.Count)];
                bibleApiUrl = $"https://bible-api.com/{selectedRef}?translation={translation}";
                selectedPrayer = _prayerMap[mood.ToLower()];
            }
            else
            {
                bibleApiUrl = $"https://bible-api.com/?random=verse&translation={translation}";
                selectedPrayer = _prayerMap["default"];
            }
            
            try 
            {
                var verseResponse = await _httpClient.GetStringAsync(bibleApiUrl);
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var verseData = JsonSerializer.Deserialize<VerseViewModel>(verseResponse, options);

                ViewData["UserFeeling"] = userFeeling; 
                ViewBag.Prayer = selectedPrayer;

                return View(verseData);
            }
            catch (Exception)
            {
                var fallback = new VerseViewModel { Reference = "Error", Text = "Could not connect to Bible API." };
                return View(fallback);
            }
        }

        // 👇 UPDATED: The Smart Streak Logic
        private void UpdateStreak()
        {
            int currentStreak = int.Parse(Request.Cookies["StreakCount"] ?? "0");
            string lastVisit = Request.Cookies["LastVisitDate"] ?? "";
            DateTime today = DateTime.Today;
            DateTime lastDate;

            bool isMissedDay = false ; // Track if they missed a day

            if (DateTime.TryParse(lastVisit, out lastDate))
            {
                if (lastDate == today) 
                {
                    // Already visited today
                }
                else if (lastDate == today.AddDays(-1)) 
                {
                    // Visited yesterday! Streak goes up
                    currentStreak++;
                }
                else 
                {
                    // Missed a day! Reset streak
                    currentStreak = 1;
                    isMissedDay = true; // 😢 They missed a day
                }
            }
            else 
            {
                // First visit ever
                currentStreak = 1;
            }

            // Save Cookies
            var options = new CookieOptions { Expires = DateTime.Now.AddYears(1) };
            Response.Cookies.Append("StreakCount", currentStreak.ToString(), options);
            Response.Cookies.Append("LastVisitDate", today.ToString("yyyy-MM-dd"), options);

            ViewBag.Streak = currentStreak;
            
            // Send this flag to the View so we can show the "Hey Bestie" banner
            ViewBag.MissedDay = isMissedDay; 
        }

        [HttpPost]
        public async Task<IActionResult> SaveVerse(VerseViewModel model, string userNote)
        {
            if (model != null)
            {
                var favorite = new FavoriteVerse
                {
                    Reference = model.Reference,
                    Text = model.Text,
                    DateSaved = DateTime.Now,
                    Note = userNote
                };
                _context.Favorites.Add(favorite);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Favorites()
        {
            var myFavorites = await _context.Favorites.OrderByDescending(f => f.DateSaved).ToListAsync();
            return View(myFavorites);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteVerse(int id)
        {
            var verse = await _context.Favorites.FindAsync(id);
            if (verse != null)
            {
                _context.Favorites.Remove(verse);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Favorites");
        }

        // PRAYER WALL
        public async Task<IActionResult> PrayerWall()
        {
            var prayers = await _context.PrayerRequests.OrderByDescending(p => p.DateCreated).ToListAsync();
            return View(prayers);
        }

        [HttpPost]
        public async Task<IActionResult> AddPrayer(string content)
        {
            if (!string.IsNullOrWhiteSpace(content))
            {
                var prayer = new PrayerRequest { Content = content };
                _context.PrayerRequests.Add(prayer);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("PrayerWall");
        }

        [HttpPost]
        public async Task<IActionResult> MarkAnswered(int id)
        {
            var prayer = await _context.PrayerRequests.FindAsync(id);
            if (prayer != null)
            {
                prayer.IsAnswered = !prayer.IsAnswered; 
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("PrayerWall");
        }

        [HttpPost]
        public async Task<IActionResult> DeletePrayer(int id)
        {
            var prayer = await _context.PrayerRequests.FindAsync(id);
            if (prayer != null)
            {
                _context.PrayerRequests.Remove(prayer);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("PrayerWall");
        }

        // READING PLANS
        public IActionResult ReadingPlans()
        {
            return View(_plans);
        }

        public async Task<IActionResult> PlanDay(string planId, int day)
        {
            var plan = _plans.FirstOrDefault(p => p.Id == planId);
            if (plan == null) return RedirectToAction("ReadingPlans");

            var planDay = plan.Days.FirstOrDefault(d => d.DayNumber == day);
            if (planDay == null) return RedirectToAction("ReadingPlans");

            string translation = Request.Cookies["BibleVersion"] ?? "web";
            string bibleApiUrl = $"https://bible-api.com/{planDay.Reference}?translation={translation}";
            
            try 
            {
                var verseResponse = await _httpClient.GetStringAsync(bibleApiUrl);
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var verseData = JsonSerializer.Deserialize<VerseViewModel>(verseResponse, options);

                ViewBag.PlanTitle = plan.Title;
                ViewBag.DayNumber = day;
                ViewBag.TotalDays = plan.Days.Count;
                ViewBag.Theme = planDay.Theme;
                ViewBag.PlanId = plan.Id;

                return View(verseData);
            }
            catch
            {
                return RedirectToAction("ReadingPlans");
            }
        }

        public IActionResult Privacy() { return View(); }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}