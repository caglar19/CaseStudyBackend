using FluentValidation;
using CaseStudy.Application.Models.Holiday;
using CaseStudy.DataAccess.Repositories;

namespace CaseStudy.Application.Models.Validators.Holiday;

public class HolidayCreateModelValidator : AbstractValidator<HolidayCreateModel>
{
    public HolidayCreateModelValidator()
    {
        // Rule for Title property in HolidayCreateModel has a minimum length of 3 characters
        RuleFor(x => x.Title)
            .MinimumLength(3)
            .WithMessage("Title must be at least 3 characters long.");
        
        // Rule for Description property in HolidayCreateModel has a minimum length of 10 characters
        RuleFor(x => x.Description)
            .MinimumLength(10)
            .WithMessage("Description must be at least 10 characters long.");
        
        // Rule for Price property in HolidayCreateModel must be greater than 0
        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Price must be 0 or greater.");
        
        // Rule for StartDate property in HolidayCreateModel must be greater than or equal to DateTime.Now
        RuleFor(x => x.StartDate)
            .GreaterThanOrEqualTo(DateTime.Now)
            .WithMessage("Start date must be greater than or equal to today.");
        
        // Rule for EndDate property in HolidayCreateModel must be greater than StartDate
        RuleFor(x => x.EndDate)
            .GreaterThan(x => x.StartDate)
            .WithMessage("End date must be greater than start date.");
        
        // Rule for Duration property in HolidayCreateModel must be greater than 0
        RuleFor(x => x.Duration)
            .GreaterThan(0)
            .WithMessage("Duration must be greater than 0.");
        
        // Rule for CategoryId must be a valid category id in the database
        //RuleFor(x => x.CategoryId)
        //    .MustAsync(async (categoryId, _) =>
        //    {
        //        var category = await categoryRepository.GetFirstOrDefaultAsync(x => x.Id == categoryId);
        //        return category != null;
        //    })
        //    .WithMessage("Category must be a valid category.");
        
        // Rule for VendorId must be a valid vendor id in the database
        //RuleFor(x => x.VendorId)
        //    .MustAsync(async (vendorId, _) =>
        //    {
        //        var vendor = await vendorRepository.GetFirstOrDefaultAsync(x => x.Id == vendorId);
        //        return vendor != null;
        //    })
        //    .WithMessage("Vendor must be a valid vendor.");
    }
}