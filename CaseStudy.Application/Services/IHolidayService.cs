using CaseStudy.Application.Models.Holiday;

namespace CaseStudy.Application.Services;

public interface IHolidayService
{
    Task<List<CountryResponseModel>> GetCountryAsync(string accessToken);
    Task<List<SubdivisionResponseModel>> GetSubdivisionAsync(SubdivisionRequestModel model);
    Task<List<HolidayResponseModel>> GetPublicHolidayAsync(HolidayRequestModel model);
    Task<List<HolidayResponseModel>> GetSchoolHolidayAsync(HolidayRequestModel model);
}
