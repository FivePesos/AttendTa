using System;

namespace ASI.Basecode.Services.ServiceModels;

/// <summary>Card model for the participant-facing published event feed.</summary>
public sealed class MemberEventListItemViewModel
{
    public int EventId { get; set; }
    public string Title { get; set; }
    public string TypeName { get; set; }
    public string VenueName { get; set; }
    public DateTime StartsAt { get; set; }
    public DateTime RegistrationDeadline { get; set; }
    public int ParticipantLimit { get; set; }
    public int ConfirmedCount { get; set; }
    public bool HasCover { get; set; }

    /// <summary>True when the canonical Signup open fact holds at the time the feed was read.</summary>
    public bool IsSignupOpen { get; set; }

    /// <summary>True when confirmed seats have reached the participant limit. Signup stays open.</summary>
    public bool IsFull => ConfirmedCount >= ParticipantLimit;
}
