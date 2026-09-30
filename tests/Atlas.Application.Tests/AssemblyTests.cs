namespace Atlas.Application.Tests;

public sealed class AssemblyTests
{
    [Fact]
    public void Application_assembly_loads()
    {
        Assert.Equal("Atlas.Application", typeof(AssemblyMarker).Assembly.GetName().Name);
    }
}
