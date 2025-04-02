using Ardalis.Result;
using Application.Features.Common.Requests;
using Application.Features.Common.Responses;
using MediatR;

namespace Application.Features.Users.GetAllUsers;
public record GetAllUsersRequest : PaginatedRequest, IRequest<Result<PaginatedList<UserResponse>>>
{

}
