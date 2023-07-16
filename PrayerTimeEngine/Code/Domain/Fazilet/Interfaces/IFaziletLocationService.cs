namespace PrayerTimeEngine.Code.Domain.Fazilet.Interfaces
{
    public interface IFaziletLocationService
    {
        Dictionary<string, int> GetCountries();
        int GetCityId(string cityName);
        Dictionary<string, int> GetCitiesByCountryId(int countryID);
    }
}
