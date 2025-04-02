using Ardalis.Result;
using Application.Features.Common.Responses;
using MediatR;

namespace Application.Features.Users.GetUserById;
public record GetUserByIdRequest(int Id) : IRequest<Result<UserResponse>>;
