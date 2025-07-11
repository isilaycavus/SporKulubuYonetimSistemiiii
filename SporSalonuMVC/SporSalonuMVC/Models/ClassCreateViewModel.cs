using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using SporSalonuMVC.Models;

namespace SporSalonuMVC.ViewModels
{
    public class ClassCreateViewModel
    {
        [Required(ErrorMessage = "Ders adı gereklidir.")]
        [Display(Name = "Ders Adı")]
        public string Name { get; set; }

        [Display(Name = "Açıklama")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Tarih ve saat gereklidir.")]
        [Display(Name = "Tarih ve Saat")]
        public DateTime Schedule { get; set; }

        [Required(ErrorMessage = "Eğitmen seçilmelidir.")]
        [Display(Name = "Eğitmen")]
        public int TrainerId { get; set; }

        [Display(Name = "Kontenjan")]
        [Range(1, 100, ErrorMessage = "Kontenjan 1 ile 100 arasında olmalıdır.")]
        public int Capacity { get; set; }

        // Eğitmen listesini sayfada dropdown'da göstermek için
        public List<Trainer> AvailableTrainers { get; set; } = new List<Trainer>();

        // Tüm branşlar (örnek: Yüzme, Fitness)
        public List<string> AvailableBranches { get; set; } = new List<string>
        {
            "Fitness", "Yüzme", "Pilates", "Kickboks", "Yoga", "CrossFit"
        };
    }
}
