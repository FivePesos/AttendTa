namespace ASI.Basecode.Resources.Constants;

/// <summary>
/// One bound for every bulk approval surface. The service and the controller read these members
/// so the posted form, the enforced limit and the message a reader sees cannot drift apart.
/// </summary>
public static class BulkApproval
{
    /// <summary>The largest number of records one bulk approval may decide.</summary>
    public const int MaxBatchSize = 25;

    /// <summary>The notice shown when a bulk approval carries no ids.</summary>
    public const string EmptySelectionMessage = "Select at least one account request to approve.";

    /// <summary>The notice shown when a bulk approval carries more ids than the bound allows.</summary>
    public static string OverBoundMessage => $"You can approve up to {MaxBatchSize} account requests at a time.";
}
