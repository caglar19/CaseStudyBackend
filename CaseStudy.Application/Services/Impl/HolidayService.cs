using CaseStudy.Application.Interfaces;
using CaseStudy.Application.Models.Holiday;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace CaseStudy.Application.Services.Impl;

public class HolidayService(HttpClient httpClient) : IHolidayService
{
    public async Task<List<CountryModel>> GetCountryAsync(string accessToken)
    {
        if (string.IsNullOrEmpty(accessToken))
        {
            throw new UnauthorizedAccessException("Access token is required.");
        }

        // Authorization başlığını ayarla
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        // API isteği yap
        var response = await httpClient.GetAsync("https://openholidaysapi.org/Countries");

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception("Failed to retrieve country codes from external API");
        }

        // Yanıtı parse et
        var countries = await response.Content.ReadFromJsonAsync<List<CountryHoliday>>();

        if (countries == null || !countries.Any())
            return new List<CountryModel>();

        // Ülke kodlarını dönüştür ve döndür
        var countryCodes = countries
            .Select(country => new CountryModel
            {
                Id = Guid.NewGuid(),
                IsoCode = country.IsoCode,
                Name = country.Name.Any(name => name.Language == "EN")
                    ? country.Name.First(name => name.Language == "EN").Text
                    : country.Name.FirstOrDefault()?.Text,
            })
            .ToList();

        return countryCodes;
    }

    public async Task<List<SubdivisionResponseModel>> GetSubdivisionAsync(SubdivisionRequestModel model)
    {
        var url = $"https://openholidaysapi.org/Subdivisions?countryIsoCode={model.CountryIsoCode}&languageIsoCode=EN";
        var response = await httpClient.GetAsync(url);

        // Eğer yanıt başarılı değilse hata fırlat
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception("Failed to retrieve subdivisions from external API");
        }

        // API yanıtını doğru türde okuyalım
        var subdivisions = await response.Content.ReadFromJsonAsync<List<Subdivision>>();

        // Eğer null dönerse boş liste döndür
        if (subdivisions == null)
        {
            return new List<SubdivisionResponseModel>();
        }
            
        // SubdivisionResponseModel listesi oluştur
        var subdivisionResponseModels = subdivisions
            .Select(subdivision => new SubdivisionResponseModel
            {
                Id = Guid.NewGuid(),
                Code = subdivision.Code,
                ShortName = subdivision.ShortName,
                LongName = subdivision.Name.Any(name => name.Language == "EN") ? subdivision.Name.First(name => name.Language == "EN").Text : subdivision.Name.FirstOrDefault()?.Text,
            })
            .ToList();
        
        return subdivisionResponseModels;
    }
    public async Task<List<HolidayResponseModel>> GetPublicHolidayAsync(HolidayRequestModel model)
    {
        var url = $"https://openholidaysapi.org/PublicHolidays?countryIsoCode={model.CountryIsoCode}&languageIsoCode=EN&validFrom={model.ValidFrom:yyyy-MM-dd}&validTo={model.ValidTo:yyyy-MM-dd}&subdivisionCode={model.SubdivisionCode}";

        var response = await httpClient.GetAsync(url);

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception("Failed to retrieve public holidays from external API");
        }

        var holidays = await response.Content.ReadFromJsonAsync<List<Holiday>>();
        
        if (holidays == null)
        {
            return new List<HolidayResponseModel>();
        }
        
        var holidayResponseModels = holidays
            .Select(holiday => new HolidayResponseModel
            {
                Id = Guid.NewGuid(),
                StartDate = holiday.StartDate.ToString("yyyy-MM-dd"),
                EndDate = holiday.EndDate.ToString("yyyy-MM-dd"),
                Name = holiday.Name.Any(name => name.Language == "EN") ? holiday.Name.First(name => name.Language == "EN").Text : holiday.Name.FirstOrDefault()?.Text,
            })
            .ToList();
        
        return holidayResponseModels;
    }
    public async Task<List<HolidayResponseModel>> GetSchoolHolidayAsync(HolidayRequestModel model)
    {
        // API URL'yi oluştur
        var url = $"https://openholidaysapi.org/SchoolHolidays?countryIsoCode={model.CountryIsoCode}&languageIsoCode=EN&validFrom={model.ValidFrom:yyyy-MM-dd}&validTo={model.ValidTo:yyyy-MM-dd}&subdivisionCode={model.SubdivisionCode}";

        // API'ye GET isteği gönder
        var response = await httpClient.GetAsync(url);

        // Başarısız cevap durumunda hata fırlat
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception("Failed to retrieve school holidays from external API");
        }

        // API'den gelen JSON cevabını deserialize et
        var schoolHolidays = await response.Content.ReadFromJsonAsync<List<Holiday>>();
        
        if (schoolHolidays == null)
        {
            return new List<HolidayResponseModel>();
        }
        
        // SchoolHolidayResponseModel listesi oluştur
        var schoolHolidayResponseModels = schoolHolidays
            .Select(holiday => new HolidayResponseModel
            {
                Id = Guid.NewGuid(),
                StartDate = holiday.StartDate.ToString("yyyy-MM-dd"),
                EndDate = holiday.EndDate.ToString("yyyy-MM-dd"),
                Name = holiday.Name.Any(name => name.Language == "EN") ? holiday.Name.First(name => name.Language == "EN").Text : holiday.Name.FirstOrDefault()?.Text,
            })
            .ToList();
        
        return schoolHolidayResponseModels;
    }
}
