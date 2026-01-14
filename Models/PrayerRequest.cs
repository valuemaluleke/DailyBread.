namespace DailyBread.Models
{
    public class PrayerRequest
    {
        public int Id { get; set; }
        
        // What are you praying for?
        public string Content { get; set; }

        // When did you start praying?
        public DateTime DateCreated { get; set; } = DateTime.Now;

        // Has God answered this yet?
        public bool IsAnswered { get; set; } = false;
    }
}