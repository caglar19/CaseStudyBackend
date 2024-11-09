using AutoMapper;
using MongoDB.Bson;
using CaseStudy.Application.Exceptions;
using CaseStudy.Application.Models;
using CaseStudy.Application.Models.Holiday;
using CaseStudy.Application.Models.Validators.Holiday;
using CaseStudy.Core.Common;
using CaseStudy.Core.Entities;
using CaseStudy.Core.Enums;
using CaseStudy.DataAccess.Repositories;
using CaseStudy.Shared.Services;
using System.Linq.Expressions;

namespace CaseStudy.Application.Services.Impl;

public class HolidayService(
    IClaimService claimService,
    IMapper mapper,
    IHolidayRepository holidayRepository
    )
    : IHolidayService
{
    private readonly string _userId = claimService.GetUserId();

    public async Task<HolidayCreateResponseModel> CreateAsync(
        HolidayCreateModel createHolidayModel)
    {
        #region Validation

        var createHolidayModelValidator =
            new HolidayCreateModelValidator();
        var validationResult =
            await createHolidayModelValidator.ValidateAsync(createHolidayModel);
        if (!validationResult.IsValid)
            throw new BadRequestException(validationResult.Errors.ToJson());

        #endregion Validation

        var holiday = mapper.Map<Holiday>(createHolidayModel);
        holiday.CreatedBy = Guid.Parse(_userId);

        var addedHoliday = await holidayRepository.AddAsync(holiday);

        // Map Holiday from addedHoliday to History<Holiday> then add it to history
        await holidayRepository.CreateHistoryAsync(
                       mapper.Map<History<Holiday>>(addedHoliday));

        return mapper.Map<HolidayCreateResponseModel>(addedHoliday);
    }

    public async Task<List<HolidayResponseModel>> GetAllAsync()
    {
        var holidays =
            await holidayRepository.GetAllAsync(predicate => predicate.DataStatus == EDataStatus.Active);
        return mapper.Map<List<HolidayResponseModel>>(holidays);
    }

    public async Task<HolidayResponseModel> GetByIdAsync(Guid refId)
    {
        var holiday = await holidayRepository.GetByRefIdAsync(refId);

        return mapper.Map<HolidayResponseModel>(holiday);
    }

    public async Task<HolidayUpdateResponseModel> UpdateAsync(Guid refId,
        HolidayUpdateModel updateHolidayModel)
    {
        #region Validation

        var updateHolidayModelValidator = new HolidayUpdateModelValidator();
        var validationResult =
            await updateHolidayModelValidator.ValidateAsync((refId, updateHolidayModel));
        if (!validationResult.IsValid)
            throw new BadRequestException(validationResult.Errors.ToJson());

        #endregion Validation

        var holiday = await holidayRepository.GetFirstAsync(csp => csp.RefId == refId);

        holiday.UpdatedBy = Guid.Parse(_userId);

        var editedHoliday =
            await holidayRepository.UpdateAsync(mapper.Map(updateHolidayModel,
                holiday));

        await holidayRepository.CreateHistoryAsync(
                                             mapper.Map<History<Holiday>>(editedHoliday));

        return mapper.Map<HolidayUpdateResponseModel>(editedHoliday);
    }

    public async Task<BaseResponseModel> DeleteAsync(Guid refId)
    {
        #region Validation

        var deleteHolidayModelValidator =
            new HolidayDeleteModelValidator();
        var validationResult = await deleteHolidayModelValidator.ValidateAsync(refId);
        if (!validationResult.IsValid)
            throw new BadRequestException(validationResult.Errors.ToJson());

        #endregion Validation

        var removedHoliday = await holidayRepository.GetFirstAsync(ip => ip.RefId == refId);

        removedHoliday.DataStatus = EDataStatus.Deleted;
        removedHoliday.DeletedBy = Guid.Parse(_userId);
        removedHoliday.DeletedOn = DateTime.Now;
        await holidayRepository.UpdateAsync(removedHoliday);

        await holidayRepository.CreateHistoryAsync(mapper.Map<History<Holiday>>(removedHoliday));

        return null;
    }

    public async Task<List<HolidayResponseModel>> GetAllHistoryAsync(int primaryKey)
    {
        Expression<Func<History<Holiday>, bool>> filter = u => u.PrimaryKey == primaryKey;
        var historyData = await holidayRepository.GetAllHistoryAsync(filter);
        return mapper.Map<List<HolidayResponseModel>>(historyData);
    }
}