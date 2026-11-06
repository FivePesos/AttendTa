using ASI.Basecode.Data.Models;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;

namespace ASI.Basecode.Data.Interfaces;

public interface IRegistrationRepository
{
    Task<bool> IsConfirmedAsync(int registrationId, CancellationToken cancellationToken = default);

    /// <summary>Reports whether the member already has a registration of any status for the event.</summary>
    Task<bool> ExistsAsync(int eventId, int memberId, CancellationToken cancellationToken = default);

    /// <summary>Counts confirmed seats for the event using the canonical seat fact.</summary>
    Task<int> CountConfirmedAsync(int eventId, CancellationToken cancellationToken = default);

    Task AddAsync(Registration registration, CancellationToken cancellationToken = default);

    Task<Registration> GetForCancellationAsync(int registrationId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Registration>> GetOpenForEventAsync(int eventId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<int>> DiscoverOpenEventIdsForUserAsync(int userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UpcomingRegistrationRow>> GetUpcomingForUserAsync(int userId, DateTime nowUtc, int? eventId = null, CancellationToken cancellationToken = default);
}

public sealed record UpcomingRegistrationRow(int RegistrationId, int EventId, string EventTitle, DateTime StartsAt);
