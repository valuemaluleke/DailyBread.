namespace DailyBread.Models
{
    // 1. A single day in a plan (e.g., "Day 1")
    public class PlanDay
    {
        public int DayNumber { get; set; }
        public string Reference { get; set; } // e.g., "Psalm 23:1"
        public string Theme { get; set; }     // e.g., "The Lord is my Shepherd"
    }

    // 2. The whole plan 
    public class ReadingPlan
    {
        public string Id { get; set; }         
        public string Title { get; set; }       
        public string Description { get; set; } 
        public string ImageEmoji { get; set; }  
        public List<PlanDay> Days { get; set; } = new List<PlanDay>();
    }
}
