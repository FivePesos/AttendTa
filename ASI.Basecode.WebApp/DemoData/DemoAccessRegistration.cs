using ASI.Basecode.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace ASI.Basecode.WebApp.DemoData;

/// <summary>
/// The membership access surface's demo registration file (register M-15b; register rule 9). It adds
/// only this surface's providers, so removing a register row means deleting its classes and its line
/// here. Membership deactivation (M-15a) was removed by 5.6 and is the real command registered by DI;
/// the demo command here delegates that half to it. Account access has no demo provider either.
/// </summary>
internal static class DemoAccessRegistration
{
    public static void AddDemoAccess(IServiceCollection services)
    {
        services.AddScoped<IMembershipAccessCommands, DemoMembershipAccessCommands>();
        services.AddScoped<IAccessStateOverlay, DemoAccessStateOverlay>();
    }
}
