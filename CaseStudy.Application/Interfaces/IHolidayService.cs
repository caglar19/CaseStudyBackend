using CaseStudy.Application.Models.Holiday;

namespace CaseStudy.Application.Interfaces;

public interface IHolidayService
{
    Task<List<CountryModel>> GetCountryAsync(string accessToken);
    Task<List<SubdivisionResponseModel>> GetSubdivisionAsync(SubdivisionRequestModel model);
    Task<List<HolidayResponseModel>> GetPublicHolidayAsync(HolidayRequestModel model);
    Task<List<HolidayResponseModel>> GetSchoolHolidayAsync(HolidayRequestModel model);
}
