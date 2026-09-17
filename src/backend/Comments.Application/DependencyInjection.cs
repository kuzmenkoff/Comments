using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Comments.Application;

/// <summary>Registers the Application layer services into the DI container.</summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Registers every AbstractValidator in this assembly.
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());


        return services;
    }
}
