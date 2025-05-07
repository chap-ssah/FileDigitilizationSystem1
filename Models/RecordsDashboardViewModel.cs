using System.Collections.Generic;

namespace FileDigitilizationSystem.Models
{
    public class RecordsDashboardViewModel
    {
        /// <summary>
        /// Number of file requests not yet handled.
        /// </summary>
        public int PendingRequests { get; set; }

        /// <summary>
        /// Number of files that have been digitized.
        /// </summary>
        public int DigitizedFiles { get; set; }

        /// <summary>
        /// Recent notifications to display (e.g., last 5 handled requests).
        /// </summary>
        public List<Notification> Notifications { get; set; } = new List<Notification>();
    }
}
