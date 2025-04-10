using Ardalis.Result;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace Application.Features.Auth.ForgotPassword;
public record ForgotPasswordRequest : IRequest<Result<bool>>
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
}
