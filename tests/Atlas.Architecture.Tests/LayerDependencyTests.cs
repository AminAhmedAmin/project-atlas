using System.Reflection;
using NetArchTest.Rules;

namespace Atlas.Architecture.Tests;

public sealed class LayerDependencyTests
{
    private const string DomainNamespace = "Atlas.Domain";
    private const string ApplicationNamespace = "Atlas.Application";
    private const string InfrastructureNamespace = "Atlas.Infrastructure";
    private const string WebNamespace = "Atlas.Web";

    private static readonly Assembly DomainAssembly = typeof(Domain.AssemblyMarker).Assembly;
    private static readonly Assembly ApplicationAssembly = typeof(Application.AssemblyMarker).Assembly;
    private static readonly Assembly InfrastructureAssembly = typeof(Infrastructure.AssemblyMarker).Assembly;
    private static readonly Assembly WebAssembly = Assembly.Load("Atlas.Web");

    [Fact]
    public void Domain_does_not_depend_on_other_layers()
    {
        var result = Types.InAssembly(DomainAssembly)
            .ShouldNot()
            .HaveDependencyOnAny(ApplicationNamespace, InfrastructureNamespace, WebNamespace)
            .GetResult();

        AssertSuccessful(result);
    }

    [Fact]
    public void Domain_does_not_depend_on_frameworks()
    {
        var result = Types.InAssembly(DomainAssembly)
            .ShouldNot()
            .HaveDependencyOnAny(
                "Microsoft.EntityFrameworkCore",
                "Microsoft.AspNetCore",
                "Microsoft.Extensions",
                "MudBlazor")
            .GetResult();

        AssertSuccessful(result);
    }

    [Fact]
    public void Application_depends_only_on_domain()
    {
        var result = Types.InAssembly(ApplicationAssembly)
            .ShouldNot()
            .HaveDependencyOnAny(InfrastructureNamespace, WebNamespace)
            .GetResult();

        AssertSuccessful(result);
    }

    [Fact]
    public void Application_does_not_depend_on_persistence_or_ui_frameworks()
    {
        var result = Types.InAssembly(ApplicationAssembly)
            .ShouldNot()
            .HaveDependencyOnAny(
                "Microsoft.EntityFrameworkCore",
                "Microsoft.AspNetCore",
                "MudBlazor")
            .GetResult();

        AssertSuccessful(result);
    }

    [Fact]
    public void Infrastructure_does_not_depend_on_web()
    {
        var result = Types.InAssembly(InfrastructureAssembly)
            .ShouldNot()
            .HaveDependencyOn(WebNamespace)
            .GetResult();

        AssertSuccessful(result);
    }

    [Theory]
    [InlineData("Atlas.Domain", new string[0])]
    [InlineData("Atlas.Application", new[] { "Atlas.Domain" })]
    [InlineData("Atlas.Infrastructure", new[] { "Atlas.Application" })]
    [InlineData("Atlas.Web", new[] { "Atlas.Application", "Atlas.Infrastructure" })]
    public void Project_references_follow_clean_architecture(string project, string[] allowedReferences)
    {
        var csproj = Path.Combine(SolutionRoot.Value, "src", project, project + ".csproj");
        var references = System.Xml.Linq.XDocument.Load(csproj)
            .Descendants("ProjectReference")
            .Select(e => Path.GetFileNameWithoutExtension(e.Attribute("Include")!.Value.Replace('\\', '/')))
            .Order()
            .ToArray();

        Assert.Equal(allowedReferences.Order().ToArray(), references);
    }

    [Fact]
    public void Domain_has_no_package_references()
    {
        var csproj = Path.Combine(SolutionRoot.Value, "src", "Atlas.Domain", "Atlas.Domain.csproj");
        var packages = System.Xml.Linq.XDocument.Load(csproj).Descendants("PackageReference");

        Assert.Empty(packages);
    }

    [Fact]
    public void Forbidden_commercial_libraries_are_not_referenced()
    {
        string[] forbidden = ["MediatR", "AutoMapper", "FluentAssertions"];
        Assembly[] assemblies = [DomainAssembly, ApplicationAssembly, InfrastructureAssembly, WebAssembly];

        var offenders = assemblies
            .SelectMany(a => a.GetReferencedAssemblies().Select(r => (Assembly: a.GetName().Name, Reference: r.Name)))
            .Where(x => forbidden.Any(f => string.Equals(f, x.Reference, StringComparison.OrdinalIgnoreCase)))
            .ToList();

        Assert.Empty(offenders);
    }

    private static readonly Lazy<string> SolutionRoot = new(() =>
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "Atlas.slnx")))
        {
            dir = dir.Parent;
        }

        return dir?.FullName ?? throw new InvalidOperationException("Could not locate Atlas.slnx.");
    });

    private static void AssertSuccessful(NetArchTest.Rules.TestResult result)
    {
        var failing = result.FailingTypeNames ?? [];
        Assert.True(result.IsSuccessful, "Layer rule violated by: " + string.Join(", ", failing));
    }
}
