using System;

namespace ASI.Basecode.Services.ServiceModels;

/// <summary>Participant-facing published event detail and its signup state.</summary>
public sealed class MemberEventDetailViewModel
{
    public int EventId { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string TypeName { get; set; }
    public string VenueName { get; set; }
    public DateTime StartsAt { get; set; }
    public DateTime RegistrationDeadline { get; set; }
    public int ParticipantLimit { get; set; }
    public int ConfirmedCount { get; set; }
    public bool HasCover { get; set; }

    /// <summary>True when the canonical Signup open fact holds at the time the detail was read.</summary>
    public bool IsSignupOpen { get; set; }

    /// <summary>True when confirmed seats have reached the participant limit. Signup stays open.</summary>
    public bool IsFull { get; set; }

    /// <summary>True when the actor already has a registration of any status for this event.</summary>
    public bool HasRegistration { get; set; }

    /// <summary>True when the actor is an eligible participant and signup is still open.</summary>
    public bool CanRegister { get; set; }
}
