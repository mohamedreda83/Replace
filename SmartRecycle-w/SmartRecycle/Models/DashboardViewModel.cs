using System.Collections.Generic;
using SmartRecycle.Models;

namespace SmartRecycle.Models
{
    public class DashboardViewModel
    {
        public User User { get; set; }
        public List<RecyclingLog> RecyclingLogs { get; set; }
        public int ItemsRecycled { get; set; }
        public string UserLevel { get; set; }
    }
}