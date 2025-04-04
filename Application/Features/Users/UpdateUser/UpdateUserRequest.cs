using Ardalis.Result;
using Application.Features.Common.Requests;
using MediatR;

namespace Application.Features.Users.UpdateUser;
public class UpdateUserRequest : IRequest<Result<bool>>
{
    public int Id { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Language { get; set; }
    public int AddressId { get; set; }
    public string? ProfileUrl { get; set; }
    public bool IsActive { get; set; }
    public string? IdentityId { get; set; }
}