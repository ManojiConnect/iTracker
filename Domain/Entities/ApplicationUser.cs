using System;
using Domain.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace Domain.Entities;

/// <summary>
/// Represents a user in the application. This class extends IdentityUser to provide additional
/// user-specific functionality and properties required by the application.
/// </summary>
public class ApplicationUser : IdentityUser, IApplicationUser
{
    /// <summary>
    /// Gets or sets the username for the user.
    /// </summary>
    public override string? UserName { get; set; }

    /// <summary>
    /// Gets or sets the email address for the user.
    /// </summary>
    public override string? Email { get; set; }

    /// <summary>
    /// Gets or sets the first name of the user.
    /// </summary>
    public required string FirstName { get; set; }

    /// <summary>
    /// Gets or sets the last name of the user.
    /// </summary>
    public required string LastName { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the user account is active.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Gets or sets the date when the user account was created.
    /// </summary>
    public DateTime CreatedDate { get; set; }

    /// <summary>
    /// Gets or sets the date when the user account was last modified.
    /// </summary>
    public DateTime? LastModifiedDate { get; set; }

    /// <summary>
    /// Gets or sets the ID of the user who created this user account.
    /// </summary>
    public required string CreatedBy { get; set; }

    /// <summary>
    /// Gets or sets the ID of the user who last modified this user account.
    /// </summary>
    public string LastModifiedBy { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the preferred language for the user. Defaults to "en" (English).
    /// </summary>
    public string Language { get; set; } = "en";

    /// <summary>
    /// Gets or sets the URL to the user's profile picture.
    /// </summary>
    public string? ProfileUrl { get; set; }
} 