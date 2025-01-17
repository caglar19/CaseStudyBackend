using CaseStudy.Application.Models;

namespace CaseStudy.Application.Interfaces;

public interface IBaseService<in TCreateModel, TCreateResponseModel, TReadResponseModel, in TUpdateModel,
    TUpdateResponseModel>
{
    Task<TCreateResponseModel> CreateAsync(TCreateModel createModel);

    Task<List<TReadResponseModel>> GetAllAsync();

    Task<TReadResponseModel> GetByIdAsync(Guid refId);

    Task<TUpdateResponseModel> UpdateAsync(Guid refId, TUpdateModel updateModel);

    Task<BaseResponseModel> DeleteAsync(Guid refId);

    Task<List<TReadResponseModel>> GetAllHistoryAsync(int primaryKey);
}