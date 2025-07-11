using Microsoft.AspNetCore.Mvc;
using SporSalonuMVC.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;

public class UserController : Controller
{
    private readonly SporSalonuDbContext _context;

    public UserController(SporSalonuDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public IActionResult Register() => View();

    [HttpPost]
    public IActionResult Register(User user)
    {
        if (!ModelState.IsValid)
        {
            var allErrors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            ViewBag.ErrorMessage = string.Join(" | ", allErrors);
            return View(user);
        }

        try
        {
            user.Role = "User";
            user.CardNumber = GenerateCardNumber();

            _context.Users.Add(user);
            _context.SaveChanges();

            return RedirectToAction("Success", new { cardNumber = user.CardNumber });
        }
        catch (Exception ex)
        {
            var msg = ex.InnerException?.Message ?? ex.Message;
            ViewBag.ErrorMessage = "HATA: " + msg;

            return View(user);
        }
    }

    [HttpGet]
    public IActionResult Login() => View();

    [HttpPost]
    public IActionResult Login(LoginViewModel loginUser)
    {
        if (!ModelState.IsValid)
            return View(loginUser);

        var user = _context.Users.FirstOrDefault(u =>
            u.CardNumber == loginUser.CardNumber && u.Password == loginUser.Password);

        if (user == null)
        {
            ModelState.AddModelError("", "Geçersiz kart numarası veya şifre.");
            return View(loginUser);
        }

        // ✅ Oturum bilgileri
        HttpContext.Session.SetString("UserCard", user.CardNumber);
        HttpContext.Session.SetString("UserRole", user.Role);
        HttpContext.Session.SetString("UserEmail", user.Email);

        return RedirectToAction("Profile");
    }

    public IActionResult Profile()
    {
        var cardNumber = HttpContext.Session.GetString("UserCard");
        if (string.IsNullOrEmpty(cardNumber))
            return RedirectToAction("Login");

        var user = _context.Users.FirstOrDefault(u => u.CardNumber == cardNumber);
        return View(user);
    }

    [HttpGet]
    public IActionResult Edit()
    {
        var cardNumber = HttpContext.Session.GetString("UserCard");
        if (string.IsNullOrEmpty(cardNumber))
            return RedirectToAction("Login");

        var user = _context.Users.FirstOrDefault(u => u.CardNumber == cardNumber);
        if (user == null)
            return NotFound();

        return View(user);
    }

    [HttpPost]
    public IActionResult Edit(User updatedUser)
    {
        if (!ModelState.IsValid)
            return View(updatedUser);

        var user = _context.Users.FirstOrDefault(u => u.Id == updatedUser.Id);
        if (user == null)
            return NotFound();

        user.FullName = updatedUser.FullName;
        user.Email = updatedUser.Email;
        user.CardNumber = updatedUser.CardNumber; // istersen kaldırılabilir

        _context.SaveChanges();

        TempData["Success"] = "Profil bilgileriniz başarıyla güncellendi.";
        return RedirectToAction("Profile");
    }

    public IActionResult Success()
    {
        var cardNumber = HttpContext.Request.Query["cardNumber"];
        ViewBag.CardNumber = cardNumber;
        return View();
    }

    [HttpPost]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Index", "Home");
    }

    // ✅ Benzersiz Kart Numarası Üreten Metot
    private string GenerateCardNumber()
    {
        var random = new Random();
        string cardNumber;

        do
        {
            cardNumber = random.Next(100000000, 999999999).ToString();
        } while (_context.Users.Any(u => u.CardNumber == cardNumber));

        return cardNumber;
    }
}
