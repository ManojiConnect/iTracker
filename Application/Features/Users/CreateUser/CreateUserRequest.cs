using Ardalis.Result;
using Application.Features.Common.Requests;
using MediatR;

namespace Application.Features.Users.CreateUser;
public class CreateUserRequest : IRequest<Result<string>>
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Language { get; set; }
    public string? Role { get; set; }
    public string? ProfileUrl { get; set; }
    public bool IsActive { get; set; } = true;
}