using CaseStudy.Application.Models;
using CaseStudy.Application.Models.Holiday;
using static CaseStudy.Application.Services.Impl.HolidayService;

namespace CaseStudy.Application.Services;

public interface IHolidayService
{

    Task<List<string>> GetCountryCodesAsync();
    Task<List<LanguageWithCode>> GetLanguagesAsync();
    Task<List<Subdivision>> GetSubdivisionsAsync(string countryIsoCode, string languageIsoCode);
    Task<List<HolidayResponseModel>> GetPublicHolidaysAsync(
    string countryIsoCode,
    string languageIsoCode,
    DateTime validFrom,
    DateTime validTo,
    string subdivisionCode);
    Task<List<SchoolHolidayResponseModel>> GetSchoolHolidaysAsync(
      string countryIsoCode,
      string languageIsoCode,
      DateTime validFrom,
      DateTime validTo,
      string subdivisionCode);
}
