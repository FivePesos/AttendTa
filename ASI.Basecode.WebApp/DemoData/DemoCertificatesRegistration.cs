using ASI.Basecode.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ASI.Basecode.WebApp.DemoData;

// M-10b removed by 6.6, M-11a by 6.7 and M-11b by 6.8: the certificate ledger is the real
// CertificateLedgerService in both switch states. With the switch on, this file registers only
// M-12's member certificate history and its shared seeded-document reader.
internal static class DemoCertificatesRegistration
{
    public static void AddCertificates(IServiceCollection services, bool enabled)
    {
        if (enabled)
        {
            services.TryAddScoped<DemoCertificateDocuments>();
            services.TryAddScoped<IMyCertificatesService, DemoMyCertificatesService>();
        }
        else
        {
            services.TryAddScoped<IMyCertificatesService, UnavailableMyCertificatesService>();
        }
    }
}
