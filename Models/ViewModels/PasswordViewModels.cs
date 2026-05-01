using System.ComponentModel.DataAnnotations;

namespace Project_385.Models.ViewModels
{
    public class ForgotPasswordViewModel
    {
        
            [Required(ErrorMessage = "Vui lòng nhập email.")]
            [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
            public string Email { get; set; }
        }
        public class ResetPasswordViewModel
        {
            [Required]
            public string UserId { get; set; }

            [Required]
            public string Token { get; set; }

            [Required(ErrorMessage = "Vui lòng nhập mật khẩu mới.")]
            [DataType(DataType.Password)]
            [MinLength(8, ErrorMessage = "Mật khẩu tối thiểu 8 ký tự.")]
            public string NewPassword { get; set; }

            [Required(ErrorMessage = "Vui lòng xác nhận mật khẩu.")]
            [DataType(DataType.Password)]
            [Compare("NewPassword", ErrorMessage = "Mật khẩu xác nhận không khớp.")]
            public string ConfirmPassword { get; set; }
        }
    }