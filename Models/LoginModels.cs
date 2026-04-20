using System.ComponentModel.DataAnnotations;

namespace Project_385.Models
{
    public class LoginModels
    {
        [Key]
        public int Id { get; set; }
        [Required(ErrorMessage = " Yêu cầu nhập Họ và tên")]
        public string FullName { get; set; }
        [Required(ErrorMessage = " Yêu cầu nhập Tên đăng nhập")]
        public string UserName { get; set; }
        [Required(ErrorMessage = " Yêu cầu nhập Email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; }
        [Required(ErrorMessage = " Yêu cầu nhập Mật khẩu")]
        public string Password { get; set; }
        public int RoleId { get; set; } = 2;
    }
}
