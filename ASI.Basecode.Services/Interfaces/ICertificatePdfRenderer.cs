using System.Threading;
using System.Threading.Tasks;

namespace ASI.Basecode.Services.Interfaces;

/// <summary>Renders the fixed development-only certificate sample.</summary>
public interface ICertificatePdfRenderer
{
    Task<byte[]> RenderSampleAsync(CancellationToken cancellationToken = default);
}
