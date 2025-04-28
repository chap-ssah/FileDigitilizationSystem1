namespace FileDigitilizationSystem.Models
{
    public class RequesterDashboardViewModel
    {
        public string UserName { get; set; }
        public IEnumerable<FileRecord> SearchResults { get; set; } = new List<FileRecord>();
        public IEnumerable<FileRequest> RecentRequests { get; set; } = new List<FileRequest>();
        public IEnumerable<Notification> Notifications { get; set; } = new List<Notification>();
        public string ApplicantName { get; set; }
        public string Province { get; set; }
        public string Location { get; set; }
        public string Notes { get; set; }

        public string SearchQuery { get; set; }
        public bool NoResults { get; set; }
    }
}
