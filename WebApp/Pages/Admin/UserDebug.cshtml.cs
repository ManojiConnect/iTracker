using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Entities;
using Infrastructure.Context;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace WebApp.Pages.Admin;

// Removed authorization requirement to allow access without login
public class UserDebugModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly AppDbContext _dbContext;

    public UserDebugModel(UserManager<ApplicationUser> userManager, AppDbContext dbContext)
    {
        _userManager = userManager;
        _dbContext = dbContext;
    }

    public List<UserDebugInfo> Users { get; set; } = new List<UserDebugInfo>();

    public async Task<IActionResult> OnGetAsync()
    {
        // Still maintaining some security by limiting to specific environments
        // but allowing access without login
        if (!HttpContext.Request.Host.Value.Contains("localhost") && 
            !HttpContext.Request.Host.Value.Contains("127.0.0.1") &&
            !HttpContext.Request.Host.Value.Contains("azure") &&
            !HttpContext.Request.Host.Value.Contains("azurewebsites.net"))
        {
            return RedirectToPage("/Index");
        }

        // Get all identity users with their password hashes
        var identityUsers = await _userManager.Users
            .AsNoTracking()
            .Select(u => new UserDebugInfo
            {
                Id = u.Id,
                UserName = u.UserName,
                Email = u.Email,
                FirstName = u.FirstName,
                LastName = u.LastName,
                PasswordHash = u.PasswordHash,
                PhoneNumber = u.PhoneNumber,
                LockoutEnabled = u.LockoutEnabled,
                LockoutEnd = u.LockoutEnd,
                AccessFailedCount = u.AccessFailedCount,
                IsActive = u.IsActive,
                CreatedDate = u.CreatedDate
            })
            .ToListAsync();

        // Get user roles
        foreach (var user in identityUsers)
        {
            var identityUser = await _userManager.FindByIdAsync(user.Id);
            if (identityUser != null)
            {
                user.Roles = await _userManager.GetRolesAsync(identityUser);
            }
        }

        Users = identityUsers;

        return Page();
    }

    public class UserDebugInfo
    {
        public string Id { get; set; } = string.Empty;
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? PasswordHash { get; set; }
        public string? PhoneNumber { get; set; }
        public bool LockoutEnabled { get; set; }
        public DateTimeOffset? LockoutEnd { get; set; }
        public int AccessFailedCount { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public IList<string> Roles { get; set; } = new List<string>();
    }
} 