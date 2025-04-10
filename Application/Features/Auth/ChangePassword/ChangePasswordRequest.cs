using Ardalis.Result;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace Application.Features.Auth.ChangePassword;
public class ChangePasswordRequest : IRequest<Result<bool>>
{
    [Required]
    public string CurrentPassword { get; set; } = string.Empty;
    
    [Required]
    public string NewPassword { get; set; } = string.Empty;
}
