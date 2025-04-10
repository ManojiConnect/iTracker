﻿namespace Application.Features.Common.Responses;
public record UserResponse
{
    public int Id { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Language { get; set; }
    public string? ProfileUrl { get; set; }
    public bool? IsActive { get; set; }
    public string? Role { get; set; }
    public int? TransactionId { get; set; }
    public string? IdentityId { get; set; }
}