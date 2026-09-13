using System.Reflection;
using NetArchTest.Rules;
using PrayerTimeEngine.Core.Application;
using PrayerTimeEngine.Core.Domain;
using PrayerTimeEngine.Core.Infrastructure;

namespace PrayerTimeEngine.Core.Tests.Unit.Architecture;

/// <summary>
/// Guards the ONION dependency rule: the inner rings (Domain, Application) must stay free
/// of infrastructure frameworks. The ring layering itself is already enforced by the csproj
/// project references; these tests additionally catch a stray package leaking inward.
/// </summary>
public class OnionArchitectureTests
{
    private static readonly Assembly DomainAssembly = typeof(TimeTypeAttributeService).Assembly;
    private static readonly Assembly ApplicationAssembly = typeof(ApplicationServiceCollectionExtensions).Assembly;
    private static readonly Assembly InfrastructureAssembly = typeof(InfrastructureServiceCollectionExtensions).Assembly;

    private static readonly string[] InfrastructureFrameworks =
    [
        "Microsoft.EntityFrameworkCore",
        "Refit",
        "HtmlAgilityPack",
        "Microsoft.Extensions.Http",
        "SQLitePCLRaw",
    ];

    [Fact]
    public void Domain_should_not_depend_on_infrastructure_frameworks()
    {
        TestResult result = Types.InAssembly(DomainAssembly)
            .ShouldNot().HaveDependencyOnAny(InfrastructureFrameworks)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: "the Domain ring must be technology-free, but these types leak an infrastructure framework: {0}",
            becauseArgs: string.Join(", ", result.FailingTypeNames ?? []));
    }

    [Fact]
    public void Application_should_not_depend_on_infrastructure_frameworks()
    {
        TestResult result = Types.InAssembly(ApplicationAssembly)
            .ShouldNot().HaveDependencyOnAny(InfrastructureFrameworks)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: "the Application ring must only orchestrate via ports, but these types leak an infrastructure framework: {0}",
            becauseArgs: string.Join(", ", result.FailingTypeNames ?? []));
    }

    [Fact]
    public void Infrastructure_contains_the_ef_core_and_refit_adapters()
    {
        // Sanity guard so the framework-name list above stays meaningful: at least the
        // repositories and Refit adapters in Infrastructure really do pull those frameworks.
        TestResult result = Types.InAssembly(InfrastructureAssembly)
            .That().HaveNameEndingWith("Repository")
            .Should().HaveDependencyOn("Microsoft.EntityFrameworkCore")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: "the EF Core repositories live in the Infrastructure ring");
    }
}
