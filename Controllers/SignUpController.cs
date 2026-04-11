using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Twit.Models;
using Twit.Models.ViewModels;
using Twit.UnitOfWork;
using Twit.Repository.DBContext;

namespace Twit.Controllers;

[AllowAnonymous]
public class SignUpController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ApplicationDbContext _context;

    public SignUpController(
        UserManager<ApplicationUser> userManager,
        IUnitOfWork unitOfWork,
        ApplicationDbContext context)
    {
        _userManager = userManager;
        _unitOfWork = unitOfWork;
        _context = context;
    }

    [HttpGet("/signup")]
    public IActionResult Index()
    {
        return View(new RegisterViewModel());
    }

    [HttpPost("/signup")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid) return View("Index", model);

        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var user = new ApplicationUser
            {
                UserName = model.UserName,
                Email = model.Email,
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                await transaction.RollbackAsync();
                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);
                return View("Index", model);
            }

            var profile = new UserProfile
            {
                UserId = user.Id,
                FirstName = model.FirstName,
                LastName = model.LastName,
                DOB = model.DOB.HasValue ? DateTime.SpecifyKind(model.DOB.Value, DateTimeKind.Utc) : null,
            };

            await _unitOfWork.UserProfileRepo.Add(profile);
            await transaction.CommitAsync();

            TempData["RegistrationSuccess"] = true;
            return RedirectToAction("Index");
        }
        catch
        {
            await transaction.RollbackAsync();
            ModelState.AddModelError(string.Empty, "Registration failed. Please try again.");
            return View("Index", model);
        }
    }
}
