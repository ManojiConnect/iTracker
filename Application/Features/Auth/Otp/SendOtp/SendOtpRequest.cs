using Ardalis.Result;
using MediatR;

namespace Application.Features.Auth.Otp.SendOtp;
public record SendOtpRequest(string Email) : IRequest<Result<bool>>;
