using System;
using System.ComponentModel.DataAnnotations;

namespace WebApp.Controllers;

public class UserViewModel
{
    public string Id { get; set; }
    
    [Required]
    [Display(Name = "Username")]
    public string UserName { get; set; }
    
    [Required]
    [EmailAddress]
    [Display(Name = "Email")]
    public string Email { get; set; }
    
    [Required]
    [Display(Name = "First Name")]
    public string FirstName { get; set; }
    
    [Required]
    [Display(Name = "Last Name")]
    public string LastName { get; set; }
    
    [Display(Name = "Role")]
    public string Role { get; set; }
    
    [Display(Name = "Is Active")]
    public bool IsActive { get; set; }
    
    [Display(Name = "Created Date")]
    public DateTime CreatedDate { get; set; }
} 