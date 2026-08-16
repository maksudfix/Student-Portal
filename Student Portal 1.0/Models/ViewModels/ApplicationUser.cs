using Microsoft.AspNetCore.Identity;

namespace Student_Portal_1._0.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? StudentInfoId { get; set; }
        public StudentInfo? StudentInfo { get; set; }
    }
}