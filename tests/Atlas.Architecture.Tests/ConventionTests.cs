using System.Reflection;
using Atlas.Application.Abstractions;
using Atlas.Application.Common;
using Atlas.Domain.Common;

namespace Atlas.Architecture.Tests;

public sealed class ConventionTests
{
    private static readonly Type[] HandlerInterfaces =
    [
        typeof(ICommandHandler<>),
        typeof(ICommandHandler<,>),
        typeof(IQueryHandler<,>),
        typeof(IValidator<>),
    ];

    [Fact]
    public void Handlers_and_validators_are_sealed()
    {
        var offenders = typeof(Application.AssemblyMarker).Assembly.GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false })
            .Where(t => t.GetInterfaces().Any(i => i.IsGenericType && HandlerInterfaces.Contains(i.GetGenericTypeDefinition())))
            .Where(t => !t.IsSealed)
            .Select(t => t.FullName)
            .ToList();

        Assert.Empty(offenders);
    }

    [Fact]
    public void Domain_entities_do_not_expose_public_setters()
    {
        var offenders = typeof(Domain.AssemblyMarker).Assembly.GetTypes()
            .Where(t => t.IsSubclassOf(typeof(Entity)))
            .SelectMany(t => t.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            .Where(p => p.SetMethod is { IsPublic: true })
            .Select(p => $"{p.DeclaringType?.Name}.{p.Name}")
            .ToList();

        Assert.Empty(offenders);
    }
}
