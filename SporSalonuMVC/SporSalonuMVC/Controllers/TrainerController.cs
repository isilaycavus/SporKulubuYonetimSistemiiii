using Microsoft.AspNetCore.Mvc;
using SporSalonuMVC.Models;

public class TrainerController : Controller
{
    private readonly SporSalonuDbContext _context;

    public TrainerController(SporSalonuDbContext context)
    {
        _context = context;
    }

    public IActionResult Index()
    {
        var trainers = _context.Trainers.ToList();
        return View(trainers);
    }

    [HttpGet]
    public IActionResult Create()
    {
        if (!UserIsAdmin())
            return RedirectToAction("Login", "User");

        return View();
    }

    [HttpPost]
    public IActionResult Create(Trainer trainer)
    {
        if (!UserIsAdmin())
            return RedirectToAction("Login", "User");

        if (!ModelState.IsValid)
            return View(trainer);

        _context.Trainers.Add(trainer);
        _context.SaveChanges();
        return RedirectToAction("Index");
    }

    private bool UserIsAdmin()
    {
        return HttpContext.Session.GetString("UserRole") == "Admin";
    }

    public IActionResult Calendar(int id)
    {
        if (!UserIsAdmin())
            return RedirectToAction("Login", "User");

        var trainer = _context.Trainers.FirstOrDefault(t => t.Id == id);
        if (trainer == null) return NotFound();

        var classes = _context.Classes
            .Where(c => c.TrainerId == id)
            .OrderBy(c => c.Schedule)
            .ToList();

        ViewBag.TrainerName = trainer.FullName;
        return View(classes);
    }

    // ✅ Silme işlemi
    [HttpPost]
    public IActionResult Delete(int id)
    {
        if (!UserIsAdmin())
            return RedirectToAction("Login", "User");

        var trainer = _context.Trainers.FirstOrDefault(t => t.Id == id);
        if (trainer == null)
        {
            TempData["Error"] = "Eğitmen bulunamadı.";
            return RedirectToAction("Index");
        }

        var hasClasses = _context.Classes.Any(c => c.TrainerId == id);
        if (hasClasses)
        {
            TempData["Error"] = "Bu eğitmenin dersleri bulunduğu için silinemez.";
            return RedirectToAction("Index");
        }

        _context.Trainers.Remove(trainer);
        _context.SaveChanges();

        TempData["Success"] = "Eğitmen başarıyla silindi.";
        return RedirectToAction("Index");
    }
}
