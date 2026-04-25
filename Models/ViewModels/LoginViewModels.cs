using System.ComponentModel.DataAnnotations;

namespace Project_385.Models.ViewModels
{
    public class LoginViewModels
    {
        [Required(ErrorMessage = "Name is required.")]
        public string UserName { get; set; }

        [DataType(DataType.Password), Required(ErrorMessage = "Password is required.")]
        public string Password { get; set; }


        public string ReturnUrl { get; set; }
    }
}
