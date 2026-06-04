using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Twit.Models;
using Twit.Models.ViewModels;
using Twit.UnitOfWork;

namespace Twit.Controllers;

[Authorize]
public class SettingsController : Controller
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<ApplicationUser> _userManager;

    public SettingsController(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager)
    {
        _unitOfWork = unitOfWork;
        _userManager = userManager;
    }

    private async Task<string?> GetCurrentUserProfileId()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return null;

        var userProfile = await _unitOfWork.UserProfileRepo.GetAll()
            .FirstOrDefaultAsync(up => up.UserId == userId);

        return userProfile?.Id;
    }

    public async Task<IActionResult> Index()
    {
        var profileId = await GetCurrentUserProfileId();
        if (profileId == null)
            return RedirectToAction("LoginPage", "Login");

        var profile = await _unitOfWork.UserProfileRepo.Get(profileId);
        if (profile == null)
            return NotFound();

        var user = await _userManager.GetUserAsync(User);
        if (user != null)
        {
            ViewBag.Email = user.Email;
            ViewBag.Username = user.UserName;
        }

        var model = new SettingsViewModel
        {
            FirstName = profile.FirstName,
            LastName = profile.LastName,
            Bio = profile.Bio,
            Location = profile.Location,
            Website = profile.Website,
            DOB = profile.DOB,
            ProfileImage = profile.ProfileImage,
            CoverImage = profile.CoverImage
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(SettingsViewModel model)
    {
        var profileId = await GetCurrentUserProfileId();
        if (profileId == null)
            return RedirectToAction("LoginPage", "Login");

        var profile = await _unitOfWork.UserProfileRepo.Get(profileId);
        if (profile == null)
            return NotFound();

        var user = await _userManager.GetUserAsync(User);

        if (!ModelState.IsValid)
        {
            if ((ModelState.ContainsKey(nameof(model.CurrentPassword)) && ModelState[nameof(model.CurrentPassword)]?.Errors.Count > 0) ||
                (ModelState.ContainsKey(nameof(model.NewPassword)) && ModelState[nameof(model.NewPassword)]?.Errors.Count > 0) ||
                (ModelState.ContainsKey(nameof(model.ConfirmNewPassword)) && ModelState[nameof(model.ConfirmNewPassword)]?.Errors.Count > 0))
            {
                ViewBag.ActivePanel = "account";
            }
            if (user != null)
            {
                ViewBag.Email = user.Email;
                ViewBag.Username = user.UserName;
            }
            return View("Index", model);
        }

        profile.FirstName = model.FirstName;
        profile.LastName = model.LastName;
        profile.Bio = model.Bio;
        profile.Location = model.Location;
        profile.Website = model.Website;
        profile.DOB = model.DOB.HasValue ? DateTime.SpecifyKind(model.DOB.Value, DateTimeKind.Utc) : null;
        profile.ProfileImage = model.ProfileImage;
        profile.CoverImage = model.CoverImage;

        await _unitOfWork.UserProfileRepo.Update(profile);
        await _unitOfWork.SaveChangesAsync();

        // Handle password change if current & new password fields are provided
        if (!string.IsNullOrEmpty(model.CurrentPassword) && !string.IsNullOrEmpty(model.NewPassword))
        {
            if (user != null)
            {
                var passwordResult = await _userManager.ChangePasswordAsync(
                    user, model.CurrentPassword, model.NewPassword);

                if (!passwordResult.Succeeded)
                {
                    foreach (var error in passwordResult.Errors)
                        ModelState.AddModelError(string.Empty, error.Description);

                    ViewBag.ActivePanel = "account";
                    if (user != null)
                    {
                        ViewBag.Email = user.Email;
                        ViewBag.Username = user.UserName;
                    }
                    return View("Index", model);
                }
            }
        }

        TempData["SettingsSuccess"] = true;
        return RedirectToAction("Index");
    }
}