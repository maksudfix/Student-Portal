using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Student_Portal_1._0.Models.ViewModels
{
    public class RegisterViewModel
    {
        [Required]
        public string StudentId { get; set; } = string.Empty;

        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;

        public string Department { get; set; } = string.Empty;
        public string Semester { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public IFormFile? ImageFile { get; set; }
    }
}