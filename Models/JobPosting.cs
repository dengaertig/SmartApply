namespace SmartApply.Models
{
    public class JobPosting
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Title { get; set; } = "";
        public string Company { get; set; } = "";
        public string Location { get; set; } = "";
        public string Address { get; set; } = "";
        public string ContactPerson { get; set; } = "";    // neu
        public string Description { get; set; } = "";
        public string Url { get; set; } = "";
        public bool IsSelected { get; set; }
        public string CoverLetter { get; set; } = "";      // neu
    }
}
