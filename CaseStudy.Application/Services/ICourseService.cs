using CaseStudy.Application.Models;
using CaseStudy.Application.Models.Holiday;

namespace CaseStudy.Application.Services;

public interface IHolidayService
{
    Task<HolidayCreateResponseModel> CreateAsync(HolidayCreateModel createModel);

    Task<List<HolidayResponseModel>> GetAllAsync();
    Task<HolidayResponseModel> GetByIdAsync(Guid refId);
    Task<HolidayUpdateResponseModel> UpdateAsync(Guid refId, HolidayUpdateModel updateModel);

    Task<BaseResponseModel> DeleteAsync(Guid refId);

    Task<List<HolidayResponseModel>> GetAllHistoryAsync(int primaryKey);
}
