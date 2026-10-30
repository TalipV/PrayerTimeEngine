using PrayerTimeEngine.Core.Domain.PlaceManagement.Models;

namespace PrayerTimeEngine.Presentation.Pages.Main;

/// <summary>
/// One row in the custom place-search dropdown. A row is either a selectable place
/// (a favorite or a normal search result) or a non-selectable separator between the two groups.
/// </summary>
public class PlaceSearchResultItem
{
    public string DisplayText { get; set; } = string.Empty;

    /// <summary>True for a saved favorite (shown with a star at the top of the list).</summary>
    public bool IsFavorite { get; set; }

    /// <summary>True for the divider row between favorites and normal results.</summary>
    public bool IsSeparator { get; set; }

    /// <summary>The underlying place. Null for a separator row.</summary>
    public BasicPlaceInfo? Place { get; set; }
}
