using MedAssist.Application.Abstractions;
using MedAssist.Application.Services;
using MedAssist.Infrastructure.Data;
using MedAssist.Infrastructure.Security;
using MedAssist.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MedAssist.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMedAssistDb(configuration);
        services.AddSingleton<ICurrentUserContext, StubCurrentUserContext>();

        services.AddScoped<IRegistrationService, RegistrationService>();
        services.AddScoped<IProfileService, ProfileService>();
        services.AddScoped<IPatientService, PatientService>();
        services.AddScoped<IConsentService, ConsentService>();
        services.AddScoped<IReferenceService, ReferenceService>();
        services.AddScoped<IDoctorService, DoctorService>();
        services.AddScoped<IPatientDirectoryService, PatientDirectoryService>();
        services.AddScoped<IStaticContentService, StaticContentService>();
        services.AddScoped<ISystemSettingsService, SystemSettingsService>();
        services.AddScoped<IBotChatService, BotChatService>();
        services.AddScoped<IBillingService, BillingService>();

        return services;
    }
}
