using System.ComponentModel.DataAnnotations;

namespace SporSalonuMVC.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Kart numarası gereklidir.")]
        [Display(Name = "Kart Numarası")]
        public string CardNumber { get; set; }

        [Required(ErrorMessage = "Şifre gereklidir.")]
        [DataType(DataType.Password)]
        [Display(Name = "Şifre")]
        public string Password { get; set; }
    }
}
