namespace AgroConnect.Domain.Entities
{
    public class FarmerProfile
    {
        public int Id { get; set; }
        public string ApplicationUserId { get; set; } = string.Empty;
        public ApplicationUser? ApplicationUser { get; set; }

        public string? District { get; set; }
        public string? Village { get; set; }
        public string? FarmType { get; set; }
        public int ExperienceYears { get; set; }
        public string? IDCardImagePath { get; set; }
        public string? FarmImagePath { get; set; }
        public string? About { get; set; }
        
        // Status of application
        public ApplicationStatus Status { get; set; } = ApplicationStatus.Pending;
    }

    public enum ApplicationStatus
    {
        Pending,
        Approved,
        Rejected
    }
}
