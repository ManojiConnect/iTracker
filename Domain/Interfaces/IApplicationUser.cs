using System;

namespace Domain.Interfaces;

public interface IApplicationUser
{
    string Id { get; set; }
    string? UserName { get; set; }
    string? Email { get; set; }
    string FirstName { get; set; }
    string LastName { get; set; }
    DateTime CreatedDate { get; set; }
    DateTime? LastModifiedDate { get; set; }
    string CreatedBy { get; set; }
    string LastModifiedBy { get; set; }
    string Language { get; set; }
    string? ProfileUrl { get; set; }
} 