using System;
using System.Collections.Generic;
using System.Linq;

namespace ASI.Basecode.WebApp.Models;

/// <summary>One row of a bulk result: the server-resolved label, whether it succeeded, and why.</summary>
public sealed record BulkResultItem(string Label, bool Succeeded, string Message);

/// <summary>
/// A surface-agnostic report of a bulk action. The surface supplies the title and the two
/// outcome words; counts are derived so no caller can report a total it did not receive.
/// </summary>
public sealed class BulkResultViewModel
{
    public BulkResultViewModel(
        string title,
        IReadOnlyList<BulkResultItem> items,
        string successLabel = "Succeeded",
        string failureLabel = "Not approved")
    {
        Title = title;
        Items = items ?? Array.Empty<BulkResultItem>();
        SuccessLabel = successLabel;
        FailureLabel = failureLabel;
    }

    public string Title { get; }
    public IReadOnlyList<BulkResultItem> Items { get; }
    public string SuccessLabel { get; }
    public string FailureLabel { get; }
    public int TotalCount => Items.Count;
    public int SucceededCount => Items.Count(item => item.Succeeded);
    public int FailedCount => TotalCount - SucceededCount;
}
