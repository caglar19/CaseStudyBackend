using CaseStudy.Application.Models.Holiday;
using System.Net.Http.Json;

namespace CaseStudy.Application.Services.Impl;

public class HolidayService : IHolidayService
{
    private readonly HttpClient _httpClient;

    public HolidayService(
        HttpClient httpClient)
    {
        _httpClient = httpClient;
    }


    public async Task<List<string>> GetCountryCodesAsync()
    {
        var response = await _httpClient.GetAsync("https://openholidaysapi.org/Countries");

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception("Failed to retrieve country codes from external API");
        }

        var countries = await response.Content.ReadFromJsonAsync<List<Country>>();

        if (countries == null)
            return new List<string>();

        // Ülke kodlarını (isoCode) döndür
        var countryCodes = countries.Select(c => c.IsoCode).ToList();
        return countryCodes;
    }

    public async Task<List<LanguageWithCode>> GetLanguagesAsync()
    {
        var response = await _httpClient.GetAsync("https://openholidaysapi.org/Languages");

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception("Failed to retrieve languages from external API");
        }

        // JSON'dan dil verilerini çeviriyoruz
        var languages = await response.Content.ReadFromJsonAsync<List<Language>>();

        // Eğer veriler null ise boş bir liste döndür
        if (languages == null)
        {
            return new List<LanguageWithCode>();
        }

        // Burada her dilin isoCode'u ve İngilizce adını alıyoruz
        var languageNames = languages
            .Select(lang => new LanguageWithCode
            {
                IsoCode = lang.IsoCode,
                Name = lang.Name.FirstOrDefault(name => name.Language == "EN")?.Text
            })
            .Where(lang => lang.Name != null) // Eğer İngilizce adı varsa listeye dahil et
            .ToList();

        return languageNames;
    }

    public async Task<List<Subdivision>> GetSubdivisionsAsync(string countryIsoCode, string languageIsoCode)
    {
        var url = $"https://openholidaysapi.org/Subdivisions?countryIsoCode={countryIsoCode}&languageIsoCode={languageIsoCode}";
        var response = await _httpClient.GetAsync(url);

        // Eğer yanıt başarılı değilse hata fırlat
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception("Failed to retrieve subdivisions from external API");
        }

        // API yanıtını doğru türde okuyalım
        var subdivisions = await response.Content.ReadFromJsonAsync<List<Subdivision>>();

        // Eğer dönüş null ise boş bir liste döndür
        return subdivisions ?? new List<Subdivision>();
    }
    public async Task<List<HolidayResponseModel>> GetPublicHolidaysAsync(
    string countryIsoCode,
    string languageIsoCode,
    DateTime validFrom,
    DateTime validTo,
    string subdivisionCode)
    {
        var url = $"https://openholidaysapi.org/PublicHolidays?countryIsoCode={countryIsoCode}&languageIsoCode={languageIsoCode}&validFrom={validFrom:yyyy-MM-dd}&validTo={validTo:yyyy-MM-dd}&subdivisionCode={subdivisionCode}";

        var response = await _httpClient.GetAsync(url);

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception("Failed to retrieve public holidays from external API");
        }

        var holidays = await response.Content.ReadFromJsonAsync<List<HolidayResponseModel>>();
        return holidays ?? new List<HolidayResponseModel>();
    }
    public async Task<List<SchoolHolidayResponseModel>> GetSchoolHolidaysAsync(
     string countryIsoCode,
     string languageIsoCode,
     DateTime validFrom,
     DateTime validTo,
     string subdivisionCode)
    {
        // API URL'yi oluştur
        var url = $"https://openholidaysapi.org/SchoolHolidays?countryIsoCode={countryIsoCode}&languageIsoCode={languageIsoCode}&validFrom={validFrom:yyyy-MM-dd}&validTo={validTo:yyyy-MM-dd}&subdivisionCode={subdivisionCode}";

        // API'ye GET isteği gönder
        var response = await _httpClient.GetAsync(url);

        // Başarısız cevap durumunda hata fırlat
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception("Failed to retrieve school holidays from external API");
        }

        // API'den gelen JSON cevabını deserialize et
        var schoolHolidays = await response.Content.ReadFromJsonAsync<List<SchoolHolidayResponseModel>>();

        // Eğer null dönerse boş liste döndür
        return schoolHolidays ?? new List<SchoolHolidayResponseModel>();
    }

}
