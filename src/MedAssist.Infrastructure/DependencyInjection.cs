using MedAssist.Application.Abstractions;
using MedAssist.Application.Services;
using MedAssist.Infrastructure.Persistence;
using MedAssist.Infrastructure.Security;
using MedAssist.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace MedAssist.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<InMemoryDataStore>();
        services.AddSingleton<ICurrentUserContext, StubCurrentUserContext>();

        services.AddScoped<IRegistrationService, RegistrationService>();
        services.AddScoped<IProfileService, ProfileService>();
        services.AddScoped<IPatientService, PatientService>();
        services.AddScoped<IDialogService, DialogService>();
        services.AddScoped<IMessageService, MessageService>();
        services.AddScoped<IConsentService, ConsentService>();
        services.AddScoped<IReferenceService, ReferenceService>();

        return services;
    }
}
