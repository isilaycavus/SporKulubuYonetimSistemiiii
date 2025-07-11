using System.ComponentModel.DataAnnotations;
namespace SporSalonuMVC.Models
{
    public class Trainer
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Ad Soyad")]
        public string FullName { get; set; }

        [Required]
        [Display(Name = "Uzmanlık Alanı")]
        public string Specialty { get; set; }

        [Display(Name = "Hakkında")]
        public string Description { get; set; }
    }
}
