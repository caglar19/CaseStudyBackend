namespace CaseStudy.Application.Models.Holiday;

public class HolidayCreateModel
{
    public string Title { get; set; }
    public string Description { get; set; }
    public double Price { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int Duration { get; set; }
    public int CategoryId { get; set; }
    public int VendorId { get; set; }
}

public class HolidayCreateResponseModel : BaseResponseModel
{
    public string? Title { get; init; }
    public string? Description { get; init; }
    public double Price { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public int Duration { get; init; }
    public int CategoryId { get; init; }
    public int VendorId { get; init; }
}