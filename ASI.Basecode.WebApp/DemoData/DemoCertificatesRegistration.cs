using ASI.Basecode.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ASI.Basecode.WebApp.DemoData;

// M-10b removed by 6.6 and M-11a by 6.7. With the switch on, the demo ledger provider only serves
// M-11b's officer viewer and PDF for the seeded row that has no retained file, and M-12's member
// history reads its retained file (or the seeded document). The real registrations take precedence,
// and the demo never replaces a real provider with a fallback.
internal static class DemoCertificatesRegistration
{
    public static void AddCertificates(IServiceCollection services, bool enabled)
    {
        if (enabled)
        {
            services.TryAddScoped<DemoCertificateDocuments>();
            services.Replace(ServiceDescriptor.Scoped<ICertificateLedgerService, DemoCertificateLedgerService>());
            services.TryAddScoped<IMyCertificatesService, DemoMyCertificatesService>();
        }
        else
        {
            services.TryAddScoped<IMyCertificatesService, UnavailableMyCertificatesService>();
        }
    }
}
