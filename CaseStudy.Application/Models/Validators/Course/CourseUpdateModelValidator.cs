using FluentValidation;
using CaseStudy.Application.Models.Holiday;

namespace CaseStudy.Application.Models.Validators.Holiday;

public class HolidayUpdateModelValidator : AbstractValidator<(Guid refId, HolidayUpdateModel model)>
{
    public HolidayUpdateModelValidator()
    {

    }
}