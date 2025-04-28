using FileDigitilizationSystem.Models;

public class RecordsDashboardViewModel
{
    public string UserName { get; set; }
    public IEnumerable<FileRecord> DigitizedRecords { get; set; }
    public IEnumerable<FileRequest> FileRequests { get; set; }   // your new “tasks”
    public IEnumerable<Notification> Notifications { get; set; }
    public IEnumerable<FileRequest> RecentRequests { get; set; } = new List<FileRequest>();

    //for search
    public bool SearchPerformed { get; set; }
    public string SearchQuery { get; set; }
    public string FilterProvince { get; set; }
    public string FilterLocation { get; set; }
    public string FilterType { get; set; }

}
