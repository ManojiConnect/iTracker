using Ardalis.Result;
using Application.Features.Common.Responses;
using MediatR;

namespace Application.Features.Users.GetUserByIdentityId;
public record GetUserByIdentityIdRequest(string IdentityId) : IRequest<Result<UserResponse>>; 