using System;
using System.Collections.Generic;

namespace Application.Features.Auth.Authenticate;

public class AuthenticateResponse
{
    public string Id { get; set; } = string.Empty;
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Language { get; set; }
    public string? ProfileUrl { get; set; }
    public bool IsActive { get; set; }
}
