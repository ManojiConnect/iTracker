using Application.Features.Auth.Authenticate;
using Application.Features.Common.Responses;
using Application.Features.Users.CreateUser;
using Application.MappingConfig;
using Mapster;

namespace Application.Features.Users;

public class UserMappingConfig : IMappingConfig
{
    public void ApplyConfig()
    {
        TypeAdapterConfig<Domain.Entities.User, AuthenticateResponse>
            .ForType();

        // for get by id and get all
        TypeAdapterConfig<Domain.Entities.User, UserResponse>
            .ForType();
        //.Map(dest => dest.RaisedOn, src => src.CreatedOn);


        // post
        TypeAdapterConfig<CreateUserRequest, Domain.Entities.User>
            .ForType()
            .Ignore(dest => dest.UserId);
    }
}