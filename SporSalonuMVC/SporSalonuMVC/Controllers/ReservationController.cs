using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SporSalonuMVC.Models;

public class ReservationController : Controller
{
    private readonly SporSalonuDbContext _context;

    public ReservationController(SporSalonuDbContext context)
    {
        _context = context;
    }

    public IActionResult MyReservations()
    {
        var email = HttpContext.Session.GetString("UserEmail");
        if (email == null) return RedirectToAction("Login", "User");

        var user = _context.Users.FirstOrDefault(u => u.Email == email);
        if (user == null) return RedirectToAction("Login", "User");

        var reservations = _context.Reservations
            .Include(r => r.Class)
            .ThenInclude(c => c.Trainer)
            .Where(r => r.UserId == user.Id)
            .ToList();

        return View(reservations);
    }

    [HttpPost]
    public IActionResult Create(int classId)
    {
        var email = HttpContext.Session.GetString("UserEmail");
        if (email == null) return RedirectToAction("Login", "User");

        var user = _context.Users.FirstOrDefault(u => u.Email == email);
        if (user == null) return RedirectToAction("Login", "User");

        var classItem = _context.Classes.FirstOrDefault(c => c.Id == classId);
        if (classItem == null)
        {
            TempData["Error"] = "Ders bulunamadı.";
            return RedirectToAction("Index", "Class");
        }

        // Aktif üyelik kontrolü
        var activeMembership = _context.UserMemberships
            .FirstOrDefault(um => um.UserId == user.Id && um.EndDate > DateTime.UtcNow);

        if (activeMembership == null)
        {
            TempData["Error"] = "Rezervasyon yapabilmek için aktif bir üyelik paketiniz olmalı.";
            return RedirectToAction("Index", "Class");
        }

        // Aynı derse tekrar kayıt olmasın
        var alreadyReserved = _context.Reservations.Any(r => r.ClassId == classId && r.UserId == user.Id);
        if (alreadyReserved)
        {
            TempData["Error"] = "Bu derse zaten rezervasyon yaptınız.";
            return RedirectToAction("Index", "Class");
        }

        // Aynı gün ve saatte başka bir rezervasyonu varsa engelle
        var sameTimeReservation = _context.Reservations
            .Include(r => r.Class)
            .Any(r => r.UserId == user.Id && r.Class.Schedule == classItem.Schedule);

        if (sameTimeReservation)
        {
            TempData["Error"] = "Bu tarih ve saatte başka bir rezervasyonunuz zaten var.";
            return RedirectToAction("Index", "Class");
        }

        var reservation = new Reservation
        {
            UserId = user.Id,
            ClassId = classId
        };

        _context.Reservations.Add(reservation);
        _context.SaveChanges();

        TempData["Success"] = "Rezervasyon başarıyla oluşturuldu.";
        return RedirectToAction("MyReservations");
    }

    [HttpPost]
    public IActionResult Cancel(int id)
    {
        var reservation = _context.Reservations.Find(id);
        if (reservation != null)
        {
            _context.Reservations.Remove(reservation);
            _context.SaveChanges();
        }

        return RedirectToAction("AllReservations");
    }


    public IActionResult AllReservations()
    {
        if (HttpContext.Session.GetString("UserRole") != "Admin")
            return RedirectToAction("Login", "User");

        var reservations = _context.Reservations
            .Include(r => r.User)
            .Include(r => r.Class)
                .ThenInclude(c => c.Trainer)
            .ToList();

        return View(reservations);
    }
}
