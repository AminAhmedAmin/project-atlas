using Atlas.Application.Abstractions;
using Atlas.Application.Common;
using Atlas.Application.Contact;
using Atlas.Application.Dashboard;
using Microsoft.Extensions.DependencyInjection;

namespace Atlas.Application.Tests;

public sealed class DependencyInjectionTests
{
    [Fact]
    public void AddApplication_registers_handlers_and_validators()
    {
        var services = new ServiceCollection().AddApplication();

        Assert.Contains(services, d => d.ServiceType == typeof(ICommandHandler<SubmitContactMessageCommand, Guid>));
        Assert.Contains(services, d => d.ServiceType == typeof(ICommandHandler<DeleteContactMessageCommand>));
        Assert.Contains(services, d => d.ServiceType == typeof(IQueryHandler<GetDashboardStatsQuery, DashboardStatsDto>));
        Assert.Contains(services, d => d.ServiceType == typeof(IValidator<SubmitContactMessageCommand>));
        Assert.Contains(services, d => d.ServiceType == typeof(TimeProvider));
    }
}
