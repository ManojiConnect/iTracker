using Ardalis.Result;
using Infrastructure.Common;
using Infrastructure.Identity;
using MediatR;
using Microsoft.AspNetCore.Identity;
using System.Threading;
using System.Threading.Tasks;
using Application.Common.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Auth.ChangePassword;
public class ChangePasswordHandler : IRequestHandler<ChangePasswordRequest, Result<bool>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ICurrentUserService _currentUserService;

    public ChangePasswordHandler(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, ICurrentUserService currentUserService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _currentUserService = currentUserService;
    }

    public async Task<Result<bool>> Handle(ChangePasswordRequest request, CancellationToken cancellationToken)
    {
        var email = _currentUserService.Email;
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            return Result.NotFound();
        }
        var result = await _userManager.ChangePasswordAsync(user, request.CurrentPasword, request.NewPassword);
        if (result.Succeeded)
        {
            await _signInManager.SignInAsync(user, true);
            return Result.Success(true);
        }
        else
            return false;
    }
}
