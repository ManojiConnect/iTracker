using Application.Common.Behaviors;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using WebApplication.Configurations;
public static class MediatRSetup
{
    public static IServiceCollection AddMediatRSetup(this IServiceCollection services)
    {
        services.AddMediatR((config) =>
        {
            config.RegisterServicesFromAssemblyContaining(typeof(WebApplication.IAssemblyMarker));
            config.RegisterServicesFromAssemblyContaining(typeof(Application.IAssemblyMarker));
            config.AddOpenBehavior(typeof(ValidationResultPipelineBehavior<,>));
        });
       services.AddValidatorsFromAssemblyContaining<Application.IAssemblyMarker>();

        return services;
    }
}