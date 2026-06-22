using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Twit.Models.ViewModels;
using Twit.Services;
using Twit.UnitOfWork;

namespace Twit.Controllers
{
    public class SearchController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPostService _postService;

        public SearchController(IUnitOfWork unitOfWork, IPostService postService)
        {
            _unitOfWork = unitOfWork;
            _postService = postService;
        }

        private async Task<string?> GetCurrentUserProfileId()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return null;

            var profile = await _unitOfWork.UserProfileRepo.GetAll().AsNoTracking()
                .FirstOrDefaultAsync(up => up.UserId == userId);

            return profile?.Id;
        }

        public async Task<IActionResult> Index(string? query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return View(new SearchResultsViewModel());

            query = query.Trim();
            var queryLower = query.ToLower();

            var profileId = await GetCurrentUserProfileId();
            var posts = await _postService.FetchPosts(profileId);
            var matchedPosts = posts
                .Where(p => p.Content.Contains(query, StringComparison.OrdinalIgnoreCase))
                .ToList();

            var users = await _unitOfWork.UserProfileRepo.GetAll().AsNoTracking()
                .Include(up => up.User)
                .Where(up =>
                    up.FirstName.ToLower().Contains(queryLower) ||
                    up.LastName.ToLower().Contains(queryLower) ||
                    (up.User.UserName != null && up.User.UserName.ToLower().Contains(queryLower)))
                .Take(20)
                .ToListAsync();

            var matchedUsers = users.Select(u =>
            {
                var initials = "";
                if (!string.IsNullOrEmpty(u.FirstName)) initials += u.FirstName[0];
                if (!string.IsNullOrEmpty(u.LastName)) initials += u.LastName[0];

                return new UserResultViewModel
                {
                    Id = u.Id,
                    Name = $"{u.FirstName} {u.LastName}".Trim(),
                    Handle = u.User.UserName ?? "",
                    Initials = initials.Length > 0 ? initials.ToUpper() : "?",
                    Bio = u.Bio
                };
            }).ToList();

            var model = new SearchResultsViewModel
            {
                Query = query,
                Posts = matchedPosts,
                Users = matchedUsers
            };

            return View(model);
        }
    }
}
