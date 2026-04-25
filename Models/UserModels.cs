using System.ComponentModel.DataAnnotations;

namespace Project_385.Models
{
    public class UserModels
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Name is required.")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Email is required."), EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string Email { get; set; }

        [DataType(DataType.Password),Required(ErrorMessage = "Password is required.")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Please confirm your password.")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; }
    }
}
