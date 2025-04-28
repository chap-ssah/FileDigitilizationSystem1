using System;
using System.Collections.Generic;

namespace FileDigitilizationSystem.Models
{
    public class DashboardViewModel
    {
        // Summary metrics
        public int ActiveUsers { get; set; }
        public int PendingRequests { get; set; }
        public int SecurityAlerts { get; set; }

        // Activity and alerts
        public List<ActivityItem> RecentActivities { get; set; } = new List<ActivityItem>();
        public List<AlertItem> Alerts { get; set; } = new List<AlertItem>();

        // User list for dashboard overview (initialized to avoid null)
        public List<UserViewModel> Users { get; set; } = new List<UserViewModel>();

        // Optional policy display
        public string PasswordPolicy { get; set; }
        public string PasswordPolicyText { get; set; }
    }

    public class ActivityItem
    {
        public string Icon { get; set; }
        public string Description { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class AlertItem
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
