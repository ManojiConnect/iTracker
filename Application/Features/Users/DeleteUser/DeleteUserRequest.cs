using Ardalis.Result;
using MediatR;

namespace Application.Features.Users.DeleteUser;

public record DeleteUserRequest : IRequest<Result<bool>>
{
    public int Id { get; set; }
    public DeleteUserRequest(int id)
    {
        Id = id;
    }
}
