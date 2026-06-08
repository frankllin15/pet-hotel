using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using PetHotel.BuildingBlocks.Persistence;
using PetHotel.Tenancy.Application.Abstractions;
using PetHotel.Tenancy.Application.Auth;
using PetHotel.Tenancy.Application.Configuration;
using PetHotel.Tenancy.Application.Invitations;
using PetHotel.Tenancy.Application.Provisioning;
using PetHotel.Tenancy.Domain.Ports;
using PetHotel.Tenancy.Infrastructure.Auth;
using PetHotel.Tenancy.Infrastructure.Identity;
using PetHotel.Tenancy.Infrastructure.Persistence;
using PetHotel.Tenancy.Infrastructure.Persistence.Repositories;

namespace PetHotel.Tenancy.Infrastructure;

/// <summary>Ponto único de registro DI do módulo Tenancy (docs/02).</summary>
public static class TenancyModuleExtensions
{
    public static IServiceCollection AddTenancyModule(this IServiceCollection services, string connectionString)
    {
        services.TryAddScoped<TenantAuditingInterceptor>();

        services.AddDbContext<TenancyDbContext>((serviceProvider, options) =>
            options
                .UseNpgsql(connectionString, npgsql =>
                    npgsql.MigrationsHistoryTable("__EFMigrationsHistory", TenancyDbContext.Schema))
                .AddInterceptors(serviceProvider.GetRequiredService<TenantAuditingInterceptor>()));

        // ASP.NET Core Identity sobre o TenancyDbContext.
        services
            .AddIdentityCore<ApplicationUser>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.Password.RequiredLength = 8;
                options.SignIn.RequireConfirmedAccount = false;
            })
            .AddRoles<ApplicationRole>()
            .AddEntityFrameworkStores<TenancyDbContext>()
            .AddDefaultTokenProviders();

        // Tokens de ativação/convite válidos por 3 dias (single-use via security stamp).
        services.Configure<DataProtectionTokenProviderOptions>(options =>
            options.TokenLifespan = TimeSpan.FromDays(3));

        services.AddScoped<IUnitOfWork, TenancyUnitOfWork>();
        services.AddScoped<ITenantRepository, TenantRepository>();
        services.AddScoped<ITenantConfigurationRepository, TenantConfigurationRepository>();
        services.AddScoped<ITenantQueries, TenantQueries>();

        // Identidade/auth (portas implementadas com Identity).
        services.AddScoped<IUserAccountService, UserAccountService>();
        services.AddScoped<IUserDirectory, UserDirectory>();
        services.AddSingleton<IJwtTokenIssuer, JwtTokenIssuer>();

        services.AddScoped<IValidator<ProvisionTenant>, ProvisionTenantValidator>();
        services.AddScoped<IValidator<ActivateAccount>, ActivateAccountValidator>();
        services.AddScoped<IValidator<Login>, LoginValidator>();
        services.AddScoped<IValidator<InviteUser>, InviteUserValidator>();
        services.AddScoped<IValidator<UpdateTenantConfiguration>, UpdateTenantConfigurationValidator>();

        return services;
    }
}
