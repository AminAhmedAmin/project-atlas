using Atlas.Application.Abstractions;
using Atlas.Application.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Atlas.Application;

public static class DependencyInjection
{
    private static readonly Type[] HandlerInterfaces =
    [
        typeof(ICommandHandler<>),
        typeof(ICommandHandler<,>),
        typeof(IQueryHandler<,>),
        typeof(IValidator<>),
    ];

    /// <summary>Registers every handler and validator in this assembly as a scoped service.</summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var types = typeof(DependencyInjection).Assembly.GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false, IsGenericTypeDefinition: false });

        foreach (var type in types)
        {
            var implemented = type.GetInterfaces()
                .Where(i => i.IsGenericType && HandlerInterfaces.Contains(i.GetGenericTypeDefinition()));

            foreach (var serviceType in implemented)
            {
                services.AddScoped(serviceType, type);
            }
        }

        services.TryAddSingleton(TimeProvider.System);
        return services;
    }
}
