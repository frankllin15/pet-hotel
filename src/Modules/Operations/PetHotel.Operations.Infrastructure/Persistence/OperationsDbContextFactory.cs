using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using PetHotel.SharedKernel;

namespace PetHotel.Operations.Infrastructure.Persistence;

/// <summary>Factory para o EF Tools criarem migrations sem subir o host (docs/04, docs/07).</summary>
public sealed class OperationsDbContextFactory : IDesignTimeDbContextFactory<OperationsDbContext>
{
    public OperationsDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("PETHOTEL_DB")
            ?? "Host=localhost;Port=5432;Database=pethotel;Username=postgres;Password=dev";

        var options = new DbContextOptionsBuilder<OperationsDbContext>()
            .UseNpgsql(connectionString, npgsql =>
                npgsql.MigrationsHistoryTable("__EFMigrationsHistory", OperationsDbContext.Schema))
            .Options;

        return new OperationsDbContext(options, new DesignTimeTenantContext());
    }

    private sealed class DesignTimeTenantContext : ITenantContext
    {
        public TenantId Current => throw new InvalidOperationException("Sem tenant em design-time.");
        public bool HasTenant => false;
    }
}
