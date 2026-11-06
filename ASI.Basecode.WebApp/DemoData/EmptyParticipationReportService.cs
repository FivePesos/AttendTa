using System.Threading;
using System.Threading.Tasks;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.ServiceModels;

namespace ASI.Basecode.WebApp.DemoData;

/// <summary>
/// Registered when <c>DemoData:Enabled</c> is off. The report screen keeps its shape and shows the
/// normal empty state instead of demo figures (register rule 2). (Register M-13a, M-13b, M-13c.)
/// </summary>
public sealed class EmptyParticipationReportService : IParticipationReportService
{
    public Task<ParticipationReportViewModel> GetReportAsync(int? eventId, int? page, CancellationToken cancellationToken = default) =>
        Task.FromResult(ParticipationReportViewModel.Empty());
}
