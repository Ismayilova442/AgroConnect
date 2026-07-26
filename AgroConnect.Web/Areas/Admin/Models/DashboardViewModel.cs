using AgroConnect.Domain.Entities;

namespace AgroConnect.Web.Areas.Admin.Models
{
    public class DashboardViewModel
    {
        // Statistika kartları
        public int PendingCount { get; set; }
        public int ApprovedCount { get; set; }
        public int RejectedCount { get; set; }
        public int ProductsCount { get; set; }
        public int UsersCount { get; set; }
        public int OrdersCount { get; set; }

        // Alt hissədəki siyahılar
        public List<ApplicationUser> RecentUsers { get; set; } = new();
        public List<Product> RecentProducts { get; set; } = new();
        public List<FarmerProfile> RecentFarmerRequests { get; set; } = new();
    }
}