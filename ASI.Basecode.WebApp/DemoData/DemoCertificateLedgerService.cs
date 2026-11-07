using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Data.Models.Lookups;
using ASI.Basecode.Resources;
using ASI.Basecode.Resources.Constants;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.ServiceModels;
using ASI.Basecode.Services.Services;

namespace ASI.Basecode.WebApp.DemoData;

// M-10b removed by 6.6 and M-11a by 6.7: the queue, both decisions and issuance are the real
// CertificateLedgerService. This provider only keeps M-11b's in-memory document for the seeded
// issued row, which carries no retained file until 6.8 removes the row.
public sealed class DemoCertificateLedgerService(
    CertificateLedgerService inner,
    IRegistrationRepository registrations,
    ICertificateRepository certificates,
    IPrivateFileStore files,
    DemoStore store,
    DemoCertificateDocuments documents,
    ICurrentActor actor) : ICertificateLedgerService
{
    public Task<CertificateLedgerViewModel> GetPageAsync(
        int? eventId,
        CertificateLedgerTab tab,
        int? page,
        CancellationToken cancellationToken = default) =>
        inner.GetPageAsync(eventId, tab, page, cancellationToken);

    public Task DecideAsync(
        int registrationId,
        bool approve,
        string reason,
        CancellationToken cancellationToken = default) =>
        inner.DecideAsync(registrationId, approve, reason, cancellationToken);

    public Task IssueAsync(int registrationId, CancellationToken cancellationToken = default) =>
        inner.IssueAsync(registrationId, cancellationToken);

    public async Task<CertificateViewerViewModel> GetViewerAsync(
        int registrationId,
        CancellationToken cancellationToken = default)
    {
        RequireManagement();
        var certificate = await RequireIssuedAsync(registrationId, cancellationToken);
        var identity = await registrations.DiscoverReviewAsync(registrationId, cancellationToken);
        if (identity is null)
        {
            throw NotFound();
        }

        return new CertificateViewerViewModel
        {
            CertificateId = registrationId,
            EventId = identity.EventId,
            MemberName = certificate.RecipientNameSnapshot,
            EventTitle = certificate.EventTitleSnapshot,
            CertificateNumber = certificate.CertificateNumber
        };
    }

    public async Task<CertificateDocument> GetPdfAsync(
        int registrationId,
        CancellationToken cancellationToken = default)
    {
        RequireManagement();
        var certificate = await RequireIssuedAsync(registrationId, cancellationToken);

        // Issued rows read the retained file; only the seeded row falls back to the in-memory document.
        if (!string.IsNullOrWhiteSpace(certificate.FilePath))
        {
            return await ReadRetainedAsync(certificate, cancellationToken);
        }

        return await documents.GetPdfAsync(registrationId, cancellationToken);
    }

    private async Task<CertificateDocument> ReadRetainedAsync(Certificate certificate, CancellationToken cancellationToken)
    {
        try
        {
            await using var stream = await files.OpenReadAsync(certificate.FilePath, cancellationToken);
            using var copy = new MemoryStream();
            await stream.CopyToAsync(copy, cancellationToken);
            return new CertificateDocument(copy.ToArray(), certificate.CertificateNumber);
        }
        catch (FileNotFoundException)
        {
            throw NotFound();
        }
        catch (DirectoryNotFoundException)
        {
            throw NotFound();
        }
    }

    private async Task<Certificate> RequireIssuedAsync(int registrationId, CancellationToken cancellationToken)
    {
        var certificate = await certificates.GetByRegistrationIdAsync(registrationId, cancellationToken);
        if (certificate is null || certificate.CertificateStatusId != CertificateStatusCode.Issued)
        {
            throw NotFound();
        }

        if (string.IsNullOrWhiteSpace(certificate.FilePath) && !store.TryGetIssuedCertificate(registrationId, out _))
        {
            throw NotFound();
        }

        return certificate;
    }

    private void RequireManagement()
    {
        if (!actor.IsAuthenticated || actor.UserId <= 0 || actor.Role is not (RoleCodes.Admin or RoleCodes.Officer))
            throw new DomainException(OutcomeCode.Forbidden, "You do not have permission to manage certificates.");
    }

    private static DomainException NotFound() => new(OutcomeCode.NotFound, "Certificate not found.");
}
