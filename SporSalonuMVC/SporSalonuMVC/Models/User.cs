using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;

public class User
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Ad Soyad alanı zorunludur.")]
    public string FullName { get; set; }

    [Required(ErrorMessage = "Email alanı zorunludur.")]
    [EmailAddress(ErrorMessage = "Geçerli bir email adresi giriniz.")]
    public string Email { get; set; }

    [Required(ErrorMessage = "Şifre alanı zorunludur.")]
    public string Password { get; set; }

    [BindNever]
    [Display(Name = "Kart Numarası")]
    [StringLength(16, MinimumLength = 16, ErrorMessage = "Kart numarası 16 haneli olmalıdır.")]
    public string? CardNumber { get; set; }

    public string Role { get; set; } = "User";
}
