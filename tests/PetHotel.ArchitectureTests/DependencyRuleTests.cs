using System.Reflection;
using NetArchTest.Rules;

namespace PetHotel.ArchitectureTests;

/// <summary>
/// Trava a regra de dependência da arquitetura hexagonal como falha de build,
/// não revisão manual (docs/01, docs/06).
/// </summary>
public class DependencyRuleTests
{
    private static readonly string[] Modules = ["Tenancy", "Registry", "Health", "Booking", "Operations"];

    private const string EfCore = "Microsoft.EntityFrameworkCore";
    private const string AspNetCore = "Microsoft.AspNetCore";
    private const string Wolverine = "Wolverine";

    private static Assembly DomainOf(string module) =>
        Assembly.Load($"PetHotel.{module}.Domain");

    private static Assembly ApplicationOf(string module) =>
        Assembly.Load($"PetHotel.{module}.Application");

    public static TheoryData<string> AllModules()
    {
        var data = new TheoryData<string>();
        foreach (var module in Modules)
        {
            data.Add(module);
        }

        return data;
    }

    [Theory]
    [MemberData(nameof(AllModules))]
    public void Dominio_nao_depende_de_infraestrutura(string module)
    {
        var result = Types.InAssembly(DomainOf(module))
            .ShouldNot()
            .HaveDependencyOnAny(EfCore, AspNetCore, Wolverine, "PetHotel.BuildingBlocks")
            .GetResult();

        Assert.True(result.IsSuccessful, Describe(result));
    }

    [Theory]
    [MemberData(nameof(AllModules))]
    public void Application_nao_depende_de_infraestrutura(string module)
    {
        var result = Types.InAssembly(ApplicationOf(module))
            .ShouldNot()
            .HaveDependencyOnAny(EfCore, AspNetCore)
            .GetResult();

        Assert.True(result.IsSuccessful, Describe(result));
    }

    [Theory]
    [MemberData(nameof(AllModules))]
    public void Modulo_nao_referencia_outro_modulo(string module)
    {
        // Outros módulos: comunicação só por Contracts/eventos, nunca por Domain de outro módulo.
        var foreignDomainNamespaces = Modules
            .Where(other => other != module)
            .Select(other => $"PetHotel.{other}.Domain")
            .ToArray();

        var result = Types.InAssembly(DomainOf(module))
            .ShouldNot()
            .HaveDependencyOnAny(foreignDomainNamespaces)
            .GetResult();

        Assert.True(result.IsSuccessful, Describe(result));
    }

    private static string Describe(TestResult result) =>
        result.IsSuccessful
            ? string.Empty
            : "Tipos violando a regra:\n" + string.Join("\n", result.FailingTypeNames ?? []);
}
