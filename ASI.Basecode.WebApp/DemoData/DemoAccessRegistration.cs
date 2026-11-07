using ASI.Basecode.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace ASI.Basecode.WebApp.DemoData;

/// <summary>
/// The membership access surface's demo registration file (register M-15a, M-15b; register rule 9).
/// It adds only this surface's providers, so removing a register row means deleting its classes and
/// its line here. Account access has no demo provider: the real command registered by DI serves it.
/// </summary>
internal static class DemoAccessRegistration
{
    public static void AddDemoAccess(IServiceCollection services)
    {
        services.AddScoped<IMembershipAccessCommands, DemoMembershipAccessCommands>();
        services.AddScoped<IAccessStateOverlay, DemoAccessStateOverlay>();
    }
}
