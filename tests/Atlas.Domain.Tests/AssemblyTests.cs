namespace Atlas.Domain.Tests;

public sealed class AssemblyTests
{
    [Fact]
    public void Domain_assembly_loads()
    {
        Assert.Equal("Atlas.Domain", typeof(AssemblyMarker).Assembly.GetName().Name);
    }
}
