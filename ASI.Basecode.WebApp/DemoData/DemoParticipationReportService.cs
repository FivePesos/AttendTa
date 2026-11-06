using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Data.Models.Lookups;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.ServiceModels;

namespace ASI.Basecode.WebApp.DemoData;

/// <summary>
/// Demo per-event participation report (register M-13a, M-13b and M-13c). It composes the existing
/// event and registration reads and overlays the demo store's attendance and completion state; it
/// never writes. 7.1, 7.2 and 7.3 replace the three parts behind the same contract.
/// </summary>
public sealed class DemoParticipationReportService : IParticipationReportService
{
    private const int AllRows = int.MaxValue;

    private static readonly RegistrationStatusCode[] Statuses =
    {
        RegistrationStatusCode.Pending,
        RegistrationStatusCode.Confirmed,
        RegistrationStatusCode.Waitlisted,
        RegistrationStatusCode.Cancelled
    };

    private readonly IEventRepository _events;
    private readonly IRegistrationRepository _registrations;
    private readonly DemoStore _store;

    public DemoParticipationReportService(
        IEventRepository events,
        IRegistrationRepository registrations,
        DemoStore store)
    {
        _events = events;
        _registrations = registrations;
        _store = store;
    }

    public async Task<ParticipationReportViewModel> GetReportAsync(int? eventId, int? page, CancellationToken cancellationToken = default)
    {
        var totalEvents = await _events.CountAsync(cancellationToken);
        var events = totalEvents == 0
            ? Array.Empty<EventListItem>()
            : await _events.GetPageAsync(0, totalEvents, cancellationToken);

        var eventItem = eventId is int selectedId
            ? await _events.GetEventAsync(selectedId, cancellationToken)
            : null;

        var selector = new EventSelectorViewModel
        {
            SelectedEventId = eventItem?.EventId,
            Options = events.Select(row => new EventSelectorOption(row.EventId, row.Title)).ToList()
        };

        if (eventItem == null)
        {
            return new ParticipationReportViewModel { Selector = selector };
        }

        // A demo-completed event is still Published in SQL; only the effective status changes.
        var status = _store.IsCompleted(eventItem.EventId)
            ? EventStatusCode.Completed
            : eventItem.EventStatusId;
        var isFinal = status == EventStatusCode.Completed;

        // One existing read per status so pending, waitlisted and cancelled rows are never dropped.
        var registrations = new List<RegistrationReviewRow>();
        foreach (var registrationStatus in Statuses)
        {
            registrations.AddRange(await _registrations.GetReviewQueueAsync(
                registrationStatus, eventItem.EventId, 0, AllRows, cancellationToken));
        }

        var attendance = new Dictionary<int, DemoAttendance>();
        foreach (var row in registrations)
        {
            if (_store.TryGetAttendance(row.RegistrationId, out var record))
            {
                attendance[row.RegistrationId] = record;
            }
        }

        var confirmed = registrations.Where(row => row.Status == RegistrationStatusCode.Confirmed).ToList();
        var validAttendees = confirmed
            .Where(row => attendance.TryGetValue(row.RegistrationId, out var record) && record.IsValid)
            .ToList();

        var confirmedCount = confirmed.Count;
        var validAttendeeCount = validAttendees.Count;

        var ordered = registrations
            .OrderBy(row => row.MemberName, StringComparer.OrdinalIgnoreCase)
            .ThenBy(row => row.RegistrationId)
            .ToList();
        var pageRequest = new PageRequest(page);
        var normalizedPage = pageRequest.NormalizePageNumber(ordered.Count);
        var pageRows = ordered
            .Skip(pageRequest.GetOffset(normalizedPage))
            .Take(PageRequest.FixedPageSize)
            .ToList();

        return new ParticipationReportViewModel
        {
            Selector = selector,
            EventId = eventItem.EventId,
            EventTitle = eventItem.Title,
            StartsAt = eventItem.StartsAt,
            EventStatusId = status,
            EventStatusLabel = EventStatusDisplay.Label(status),
            EventStatusBadgeClass = EventStatusDisplay.BadgeClass(status),
            IsFinal = isFinal,
            PendingCount = registrations.Count(row => row.Status == RegistrationStatusCode.Pending),
            ConfirmedCount = confirmedCount,
            WaitlistedCount = registrations.Count(row => row.Status == RegistrationStatusCode.Waitlisted),
            CancelledCount = registrations.Count(row => row.Status == RegistrationStatusCode.Cancelled),
            ValidAttendeeCount = validAttendeeCount,
            AttendanceRate = confirmedCount == 0
                ? null
                : Math.Round((decimal)validAttendeeCount / confirmedCount, 2, MidpointRounding.AwayFromZero),
            // No-shows are a completed-event fact; a cancelled event shows no figure at all.
            NoShowCount = isFinal ? confirmedCount - validAttendeeCount : 0,
            Rows = pageRows.Select(row => ToRow(row, attendance, isFinal)).ToList(),
            CourseBreakdown = CourseBreakdown(validAttendees),
            YearBreakdown = YearBreakdown(validAttendees),
            PageNumber = normalizedPage,
            TotalCount = ordered.Count
        };
    }

    private static ParticipationReportRowViewModel ToRow(
        RegistrationReviewRow row,
        IReadOnlyDictionary<int, DemoAttendance> attendance,
        bool isFinal)
    {
        attendance.TryGetValue(row.RegistrationId, out var record);
        var isConfirmed = row.Status == RegistrationStatusCode.Confirmed;
        var hasValidAttendance = record is { IsValid: true };
        var isNoShow = isFinal && isConfirmed && !hasValidAttendance;

        var (attendanceLabel, attendanceBadge) = Attendance(record, isConfirmed, isNoShow);

        return new ParticipationReportRowViewModel
        {
            RegistrationId = row.RegistrationId,
            MemberName = row.MemberName,
            StudentNumber = row.StudentNumber,
            Course = row.Course,
            YearLevel = row.YearLevel,
            Status = row.Status,
            StatusLabel = RegistrationStatusDisplay.Label(row.Status, false),
            StatusBadgeClass = RegistrationStatusDisplay.BadgeClass(row.Status, false),
            Attended = hasValidAttendance,
            IsNoShow = isNoShow,
            CheckedInAt = record?.RecordedAt,
            AttendanceLabel = attendanceLabel,
            AttendanceBadgeClass = attendanceBadge
        };
    }

    private static (string Label, string BadgeClass) Attendance(DemoAttendance record, bool isConfirmed, bool isNoShow)
    {
        if (record is null)
        {
            if (!isConfirmed)
            {
                // Only a confirmed registration is expected at the door; other statuses carry no result.
                return ("—", null);
            }

            return isNoShow ? ("No-show", "badge-inactive") : ("Not checked in", "badge-closed");
        }

        // An invalidated check-in is still shown as Invalidated; it is not a valid attendee.
        return record.IsValid
            ? ("Checked in", "badge-open")
            : ("Invalidated", "badge-inactive");
    }

    private static IReadOnlyList<ParticipationBreakdownItemViewModel> CourseBreakdown(IReadOnlyList<RegistrationReviewRow> attendees) =>
        attendees
            .GroupBy(
                row => string.IsNullOrWhiteSpace(row.Course) ? "Unspecified" : row.Course.Trim(),
                StringComparer.OrdinalIgnoreCase)
            .Select(group => new { Label = group.Key, Count = group.Count() })
            .OrderByDescending(item => item.Count)
            .ThenBy(item => item.Label, StringComparer.OrdinalIgnoreCase)
            .Select(item => new ParticipationBreakdownItemViewModel
            {
                Label = item.Label,
                Count = item.Count,
                Percent = Share(item.Count, attendees.Count)
            })
            .ToList();

    private static IReadOnlyList<ParticipationBreakdownItemViewModel> YearBreakdown(IReadOnlyList<RegistrationReviewRow> attendees) =>
        attendees
            .GroupBy(row => row.YearLevel)
            .OrderBy(group => group.Key)
            .Select(group => new ParticipationBreakdownItemViewModel
            {
                Label = group.Key > 0 ? $"Year {group.Key}" : "Unspecified",
                Count = group.Count(),
                Percent = Share(group.Count(), attendees.Count)
            })
            .ToList();

    private static int Share(int count, int total) =>
        total == 0 ? 0 : (int)Math.Round(count * 100m / total, MidpointRounding.AwayFromZero);
}
