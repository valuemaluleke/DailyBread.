namespace DailyBread.Models
{
    public class FavoriteVerse
    {
        public int Id { get; set; }           // A unique ID number
        public string Reference { get; set; } // e.g. "John 3:16"
        public string Text { get; set; }      // The verse itself
        public DateTime DateSaved { get; set; } = DateTime.Now; // The date you clicked save

        public string? Note { get; set; }   // An optional personal note
    }
}

