using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SporSalonuMVC.Models;
using SporSalonuMVC.ViewModels;

public class ClassController : Controller
{
    private readonly SporSalonuDbContext _context;

    public ClassController(SporSalonuDbContext context)
    {
        _context = context;
    }

    private bool UserIsAdmin()
    {
        return HttpContext.Session.GetString("UserRole") == "Admin";
    }

    public IActionResult Index()
    {
        var now = DateTime.UtcNow;

        var classes = _context.Classes
            .Include(c => c.Trainer)
            .Include(c => c.Reservations)
            .Where(c => c.Schedule > now)
            .OrderBy(c => c.Schedule)
            .ToList();

        foreach (var cls in classes)
        {
            cls.Reservations ??= new List<Reservation>();
        }

        var email = HttpContext.Session.GetString("UserEmail");
        var user = _context.Users.FirstOrDefault(u => u.Email == email);

        // Kullanıcıysa ve aktif üyeliği yoksa uyarı ver
        if (!UserIsAdmin() && user != null)
        {
            var hasActiveMembership = _context.UserMemberships
                .Any(um => um.UserId == user.Id && um.EndDate >= DateTime.UtcNow);

            if (!hasActiveMembership)
            {
                TempData["Error"] = "Rezervasyon yapabilmek için aktif bir üyelik paketiniz olmalı.";
            }
        }

        var reservedClassIds = user != null
            ? _context.Reservations
                .Where(r => r.UserId == user.Id)
                .Select(r => r.ClassId)
                .ToList()
            : new List<int>();

        ViewBag.UserReservedClassIds = reservedClassIds;
        ViewBag.IsAdmin = UserIsAdmin();

        return View(classes);
    }

    [HttpGet]
    public IActionResult Create()
    {
        if (!UserIsAdmin())
            return RedirectToAction("Login", "User");

        var model = new ClassCreateViewModel
        {
            AvailableTrainers = _context.Trainers.ToList()
        };

        return View(model);
    }

    [HttpPost]
    public IActionResult Create(ClassCreateViewModel viewModel)
    {
        if (!UserIsAdmin())
            return RedirectToAction("Login", "User");

        if (viewModel.Schedule < DateTime.Now)
        {
            ModelState.AddModelError("Schedule", "Geçmiş bir tarih seçilemez.");
            viewModel.AvailableTrainers = _context.Trainers.ToList();
            return View(viewModel);
        }

        if (!ModelState.IsValid)
        {
            viewModel.AvailableTrainers = _context.Trainers.ToList();
            return View(viewModel);
        }

        try
        {
            var cls = new Class
            {
                Name = viewModel.Name,
                Description = viewModel.Description,
                Schedule = DateTime.SpecifyKind(viewModel.Schedule, DateTimeKind.Utc),
                TrainerId = viewModel.TrainerId,
                Capacity = viewModel.Capacity
            };

            _context.Classes.Add(cls);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            viewModel.AvailableTrainers = _context.Trainers.ToList();
            ModelState.AddModelError("", "Veritabanı hatası: " + (ex.InnerException?.Message ?? ex.Message));
            return View(viewModel);
        }
    }

    [HttpPost]
    public IActionResult Delete(int id)
    {
        if (!UserIsAdmin())
            return RedirectToAction("Login", "User");

        var cls = _context.Classes
            .Include(c => c.Reservations)
            .FirstOrDefault(c => c.Id == id);

        if (cls == null)
        {
            TempData["Error"] = "Ders bulunamadı.";
            return RedirectToAction("Index");
        }

        if (cls.Reservations != null && cls.Reservations.Any())
        {
            _context.Reservations.RemoveRange(cls.Reservations);
        }

        _context.Classes.Remove(cls);
        _context.SaveChanges();

        TempData["Success"] = "Ders başarıyla silindi.";
        return RedirectToAction("Index");
    }
}
