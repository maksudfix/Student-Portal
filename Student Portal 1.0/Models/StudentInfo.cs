using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Student_Portal_1._0.Models
{
    [Index(nameof(StudentId), IsUnique = true)]
    public class StudentInfo
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int StudentId { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string Department { get; set; } = string.Empty;

        [Required]
        public string Semester { get; set; } = string.Empty;

        [Required]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Phone { get; set; } = string.Empty;

        [Required]
        public string Gender { get; set; } = string.Empty;

        [Required]
        [MaxLength(1000, ErrorMessage = "Address cannot exceed 1000 characters.")]
        public string Address { get; set; } = string.Empty;

        public string? Image { get; set; }

        public string? ApplicationUserId { get; set; }
        public ApplicationUser? ApplicationUser { get; set; }
    }
}