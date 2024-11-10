using CaseStudy.Application.Models.Holiday;
using System.Net.Http.Json;

namespace CaseStudy.Application.Services.Impl;

public class HolidayService(HttpClient httpClient) : IHolidayService
{
    public async Task<List<CountryResponseModel>> GetCountryAsync()
    {
        var response = await httpClient.GetAsync("https://openholidaysapi.org/Countries");

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception("Failed to retrieve country codes from external API");
        }

        var countries = await response.Content.ReadFromJsonAsync<List<Country>>();

        if (countries == null)
            return [];

        // Ülke kodlarını (isoCode) döndür
        var countryCodes = countries
            .Select(country => new CountryResponseModel
            {
                Id = Guid.NewGuid(),
                IsoCode = country.IsoCode,
                Name = country.Name.First(name => name.Language == "EN").Text,
            })
            .ToList();
        return countryCodes;
    }
    public async Task<List<SubdivisionResponseModel>> GetSubdivisionAsync(SubdivisionRequestModel model)
    {
        var url = $"https://openholidaysapi.org/Subdivisions?countryIsoCode={model.CountryIsoCode}&languageIsoCode={model.LanguageIsoCode}";
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
                IsoCode = subdivision.IsoCode,
                ShortName = subdivision.ShortName,
                LongName = subdivision.Name.First(name => name.Language == "EN").Text
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
                Name = holiday.Name.First(name => name.Language == "EN").Text
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
                Name = holiday.Name.First(name => name.Language == "EN").Text
            })
            .ToList();
        
        return schoolHolidayResponseModels;
    }
}
