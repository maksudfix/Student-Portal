using System.ComponentModel.DataAnnotations;

namespace Student_Portal_1._0.Models.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Enter with valid Id.")]
        public string StudentId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Wrong Password! Enter valid password.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
    }
}