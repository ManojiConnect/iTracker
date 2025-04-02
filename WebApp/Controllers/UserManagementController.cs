using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Domain.Entities;

namespace WebApp.Controllers;

[Authorize(Roles = "Admin")]
[Route("[controller]")]
public class UserManagementController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public UserManagementController(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    // GET: /UserManagement
    [HttpGet]
    [Route("")]
    [Route("Index")]
    public async Task<IActionResult> Index()
    {
        try
        {
            var users = await _userManager.Users.ToListAsync();
            var userViewModels = new List<UserViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                var userViewModel = new UserViewModel
                {
                    Id = user.Id,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    PhoneNumber = user.PhoneNumber,
                    IsActive = user.IsActive,
                    CreatedDate = user.CreatedDate,
                    Role = roles.FirstOrDefault() ?? "User" // Default to "User" if no role is assigned
                };

                userViewModels.Add(userViewModel);
            }

            return View(userViewModels);
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"An error occurred while retrieving users: {ex.Message}";
            return View(new List<UserViewModel>());
        }
    }

    // GET: /UserManagement/Create
    [HttpGet]
    [Route("Create")]
    public IActionResult Create()
    {
        return View();
    }

    // POST: /UserManagement/Create
    [HttpPost]
    [Route("Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateUserViewModel model)
    {
        if (ModelState.IsValid)
        {
            try
            {
                var existingUser = await _userManager.FindByEmailAsync(model.Email);
                if (existingUser != null)
                {
                    ModelState.AddModelError(string.Empty, "A user with this email already exists.");
                    return View(model);
                }

                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    EmailConfirmed = true,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = User.Identity?.Name ?? "System"
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    // Assign a default role if none is selected
                    string roleToAssign = !string.IsNullOrEmpty(model.Role) 
                        ? model.Role 
                        : "User"; // Default role
                    
                    await _userManager.AddToRoleAsync(user, roleToAssign);

                    TempData["SuccessMessage"] = "User created successfully!";
                    return RedirectToAction(nameof(Index));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"An error occurred: {ex.Message}");
            }
        }

        return View(model);
    }

    // GET: /UserManagement/Edit/5
    [HttpGet]
    [Route("Edit/{id}")]
    public async Task<IActionResult> Edit(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return NotFound();
        }

        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        var roles = await _userManager.GetRolesAsync(user);
        var editUserViewModel = new EditUserViewModel
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            IsActive = user.IsActive,
            Role = roles.FirstOrDefault()
        };

        return View(editUserViewModel);
    }

    // POST: /UserManagement/Edit
    [HttpPost]
    [Route("Edit/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string id, EditUserViewModel model)
    {
        if (id != model.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    return NotFound();
                }

                user.Email = model.Email;
                user.UserName = model.Email;
                user.FirstName = model.FirstName;
                user.LastName = model.LastName;
                user.IsActive = model.IsActive;
                user.LastModifiedBy = User.Identity?.Name ?? "System";
                user.LastModifiedDate = DateTime.UtcNow;

                var result = await _userManager.UpdateAsync(user);

                if (result.Succeeded)
                {
                    // If a new password is provided, update it
                    if (!string.IsNullOrWhiteSpace(model.NewPassword))
                    {
                        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                        var passwordResult = await _userManager.ResetPasswordAsync(user, token, model.NewPassword);
                        
                        if (!passwordResult.Succeeded)
                        {
                            foreach (var error in passwordResult.Errors)
                            {
                                ModelState.AddModelError(string.Empty, error.Description);
                            }
                            return View(model);
                        }
                    }

                    // Update roles
                    var currentRoles = await _userManager.GetRolesAsync(user);
                    await _userManager.RemoveFromRolesAsync(user, currentRoles);
                    
                    // Assign a default role if none is selected
                    string roleToAssign = !string.IsNullOrEmpty(model.Role) 
                        ? model.Role 
                        : "User"; // Default role
                    
                    await _userManager.AddToRoleAsync(user, roleToAssign);

                    TempData["SuccessMessage"] = "User updated successfully!";
                    return RedirectToAction(nameof(Index));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"An error occurred: {ex.Message}");
            }
        }

        return View(model);
    }

    // GET: /UserManagement/Delete/5
    [HttpGet]
    [Route("Delete/{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return NotFound();
        }

        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        var roles = await _userManager.GetRolesAsync(user);
        var userViewModel = new UserViewModel
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = roles.FirstOrDefault() ?? "No Role"
        };

        return View(userViewModel);
    }

    // POST: /UserManagement/Delete/5
    [HttpPost]
    [Route("Delete/{id}")]
    [ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        // Don't allow deleting the current user
        if (User.Identity?.Name == user.Email)
        {
            TempData["ErrorMessage"] = "You cannot delete your own account!";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            // Alternative: instead of deleting, just deactivate
            user.IsActive = false;
            await _userManager.UpdateAsync(user);

            // Or, uncomment to actually delete:
            // var result = await _userManager.DeleteAsync(user);
            // if (!result.Succeeded)
            // {
            //     TempData["ErrorMessage"] = "Failed to delete user. " + string.Join(", ", result.Errors.Select(e => e.Description));
            //     return RedirectToAction(nameof(Index));
            // }

            TempData["SuccessMessage"] = "User deactivated successfully!";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"An error occurred: {ex.Message}";
            return RedirectToAction(nameof(Index));
        }
    }
}

public class UserViewModel
{
    public string Id { get; set; }
    
    [Display(Name = "Email Address")]
    public string Email { get; set; }
    
    [Display(Name = "First Name")]
    public string FirstName { get; set; }
    
    [Display(Name = "Last Name")]
    public string LastName { get; set; }
    
    [Display(Name = "Phone Number")]
    public string PhoneNumber { get; set; }
    
    [Display(Name = "Active")]
    public bool IsActive { get; set; }
    
    [Display(Name = "Created Date")]
    public DateTime CreatedDate { get; set; }
    
    [Display(Name = "Role")]
    public string Role { get; set; }
}

public class CreateUserViewModel
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email address")]
    [Display(Name = "Email Address")]
    public string Email { get; set; }
    
    [Required(ErrorMessage = "First name is required")]
    [Display(Name = "First Name")]
    public string FirstName { get; set; }
    
    [Required(ErrorMessage = "Last name is required")]
    [Display(Name = "Last Name")]
    public string LastName { get; set; }
    
    [Required(ErrorMessage = "Password is required")]
    [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string Password { get; set; }
    
    [Display(Name = "Role")]
    public string Role { get; set; }
}

public class EditUserViewModel
{
    public string Id { get; set; }
    
    [Display(Name = "Email Address")]
    [EmailAddress(ErrorMessage = "Invalid email address")]
    public string Email { get; set; }
    
    [Required(ErrorMessage = "First name is required")]
    [Display(Name = "First Name")]
    public string FirstName { get; set; }
    
    [Required(ErrorMessage = "Last name is required")]
    [Display(Name = "Last Name")]
    public string LastName { get; set; }
    
    [Display(Name = "Active")]
    public bool IsActive { get; set; }
    
    [Display(Name = "Role")]
    public string Role { get; set; }
    
    [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
    [DataType(DataType.Password)]
    [Display(Name = "New Password (optional)")]
    public string NewPassword { get; set; }
} 