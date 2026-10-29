using System.Reflection;
using NetArchTest.Rules;

namespace Bms.ArchitectureTests;

/// <summary>
/// Ép luồng phụ thuộc Clean Architecture:
/// API → Presentation → Application → Domain → BuildingBlocks.Contracts → BuildingBlocks.Shared;
/// Infrastructure/Persistence → Contracts (+ BuildingBlocks.Infrastructure).
/// Khác QMS: dùng đúng namespace thật ("Bms.*") nên test thực sự có hiệu lực.
/// </summary>
public sealed class LayerDependencyTests
{
    private const string Domain = "Bms.Domain";
    private const string Application = "Bms.Application";
    private const string Infrastructure = "Bms.Infrastructure";
    private const string Persistence = "Bms.Persistence";
    private const string Presentation = "Bms.Presentation";
    private const string Api = "Bms.API";
    private const string BbContracts = "BuildingBlocks.Contracts";
    private const string BbInfrastructure = "BuildingBlocks.Infrastructure";
    private const string BbShared = "BuildingBlocks.Shared";
    private const string EfCore = "Microsoft.EntityFrameworkCore";
    private const string AspNetCore = "Microsoft.AspNetCore";

    public static TheoryData<string, string[]> ForbiddenDependencies => new()
    {
        { BbShared, [BbContracts, BbInfrastructure, Domain, Application, Infrastructure, Persistence, Presentation, Api, EfCore, AspNetCore] },
        { BbContracts, [BbInfrastructure, Domain, Application, Infrastructure, Persistence, Presentation, Api, EfCore, AspNetCore] },
        { BbInfrastructure, [Domain, Application, Infrastructure, Persistence, Presentation, Api] },
        { Domain, [Application, Infrastructure, Persistence, Presentation, Api, BbInfrastructure, EfCore, AspNetCore] },
        { Application, [Infrastructure, Persistence, Presentation, Api, BbInfrastructure, AspNetCore] },
        { Infrastructure, [Application, Persistence, Presentation, Api] },
        { Persistence, [Application, Infrastructure, Presentation, Api] },
        { Presentation, [Domain, Infrastructure, Persistence, Api, BbInfrastructure] },
    };

    [Theory]
    [MemberData(nameof(ForbiddenDependencies))]
    public void Layer_ShouldNotDependOn_ForbiddenLayers(string layer, string[] forbidden)
    {
        var result = Types.InAssembly(LoadAssembly(layer))
            .ShouldNot()
            .HaveDependencyOnAny(forbidden)
            .GetResult();

        Assert.True(result.IsSuccessful, FormatFailure(layer, result));
    }

    [Fact]
    public void Handlers_ShouldNotBePublic()
    {
        var result = Types.InAssembly(LoadAssembly(Application))
            .That().HaveNameEndingWith("Handler")
            .ShouldNot().BePublic()
            .GetResult();

        Assert.True(result.IsSuccessful, FormatFailure(Application, result));
    }

    [Fact]
    public void Controllers_ShouldInheritApiControllerBase()
    {
        var result = Types.InAssembly(LoadAssembly(Presentation))
            .That().HaveNameEndingWith("Controller")
            .And().AreNotAbstract()
            .Should().Inherit(typeof(global::Bms.Presentation.Abstractions.ApiControllerBase))
            .GetResult();

        Assert.True(result.IsSuccessful, FormatFailure(Presentation, result));
    }

    /// <summary>Nạp theo tên — không cần class "AssemblyReference" placeholder trong mỗi project (vd: Domain đang rỗng).</summary>
    private static Assembly LoadAssembly(string name) => Assembly.Load(name);

    private static string FormatFailure(string layer, NetArchTest.Rules.TestResult result) =>
        $"{layer} violates dependency rules: {string.Join(", ", result.FailingTypeNames ?? [])}";
}
