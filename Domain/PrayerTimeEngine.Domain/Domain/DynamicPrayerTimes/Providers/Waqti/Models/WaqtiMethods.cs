namespace PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Waqti.Models;

/// <summary>
/// The base calculation methods the Waqti backend accepts in the <c>method</c> field of a request.
/// The names are sent as they are, so they have to match the backend's spelling exactly
/// (an unknown name is answered with HTTP 500).
/// </summary>
public static class WaqtiMethods
{
    public const string DEFAULT_METHOD = "Mwl2007";

    public static readonly IReadOnlyList<string> ALL_METHODS =
    [
        "AqrabAlBiladAlNisb",
        "Polar",
        "Bosnia",
        "Mwl2007",
        "Mwl2009",
        "Ijtihad2022",
        "Ijtihad2025",
        "Diyanet",
        "Unity2023",
        "Ditib2023",
        "DumRT",
        "DumRF",
        "DumCR",
        "UmmAlQura",
        "EgyptGeneralObservatory",
        "Astronomical",
        "NientyMinutes",
        "Fifteen",
        "Twelve",
        "Dag2023",
        "Hanafi",
        "Jakim",
        "KementerianAgama",
        "Traditional",
        "Fusul",
    ];
}
