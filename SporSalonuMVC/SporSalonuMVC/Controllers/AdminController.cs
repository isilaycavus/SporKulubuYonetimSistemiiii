using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SporSalonuMVC.Models;

public class AdminController : Controller
{
    private readonly SporSalonuDbContext _context;

    public AdminController(SporSalonuDbContext context)
    {
        _context = context;
    }

    public IActionResult Dashboard()
    {
        if (HttpContext.Session.GetString("UserRole") != "Admin")
            return RedirectToAction("Login", "User");

        var memberships = _context.UserMemberships
            .Include(m => m.Membership)
            .ToList();

        var mostPopularPackage = memberships
            .Where(m => m.Membership != null)
            .GroupBy(m => m.Membership.Name)
            .OrderByDescending(g => g.Count())
            .Select(g => g.Key)
            .FirstOrDefault();

        var now = DateTime.UtcNow;
        var thisMonthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var nextMonthStart = thisMonthStart.AddMonths(1);

        var monthlyRevenue = _context.UserMemberships
            .Include(um => um.Membership)
            .Where(um => um.StartDate >= thisMonthStart && um.StartDate < nextMonthStart)
            .Sum(um => (decimal?)um.Membership.Price) ?? 0;

        var packageRevenue = _context.UserMemberships
            .Include(um => um.Membership)
            .Where(um => um.Membership != null)
            .GroupBy(um => um.Membership.Name)
            .Select(g => new
            {
                PackageName = g.Key,
                TotalRevenue = g.Sum(x => x.Membership.Price)
            })
            .ToDictionary(x => x.PackageName, x => x.TotalRevenue);

        // ✅ Tek bir model nesnesi oluşturuyoruz
        var model = new AdminDashboardViewModel
        {
            TotalUsers = _context.Users.Count(),
            TotalTrainers = _context.Trainers.Count(),
            ActiveMemberships = memberships.Count(m => m.EndDate > DateTime.UtcNow),
            TodayClasses = _context.Classes.Count(c => c.Schedule.Date == DateTime.UtcNow.Date),
            MostPopularPackage = mostPopularPackage,
            MonthlyRevenue = monthlyRevenue,
            PackageRevenue = packageRevenue
        };

        return View(model);
    }


}
