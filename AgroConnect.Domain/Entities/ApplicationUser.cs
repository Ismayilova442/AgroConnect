using Microsoft.AspNetCore.Identity;

namespace AgroConnect.Domain.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        
        // Navigation properties
        public FarmerProfile? FarmerProfile { get; set; }
    }
}
