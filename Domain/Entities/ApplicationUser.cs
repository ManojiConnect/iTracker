using System;
using Domain.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace Domain.Entities;

public class ApplicationUser : IdentityUser, IApplicationUser
{
    public override string? UserName { get; set; }
    public override string? Email { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? LastModifiedDate { get; set; }
    public required string CreatedBy { get; set; }
    public string LastModifiedBy { get; set; } = string.Empty;
    public string Language { get; set; } = "en";
    public string? ProfileUrl { get; set; }
} 