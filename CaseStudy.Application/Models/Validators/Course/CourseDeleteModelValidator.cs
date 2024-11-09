using FluentValidation;

namespace CaseStudy.Application.Models.Validators.Holiday;

public class HolidayDeleteModelValidator : AbstractValidator<Guid>
{
    public HolidayDeleteModelValidator()
    {

    }
}