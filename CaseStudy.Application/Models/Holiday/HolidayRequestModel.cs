namespace CaseStudy.Application.Models.Holiday;

public class HolidayRequestModel
{
    public required string CountryIsoCode { get; set; }
    public required DateTime ValidFrom { get; set; }
    public required DateTime ValidTo { get; set; }
    public string? SubdivisionCode { get; set; }
    public List<HolidayType> HolidayType { get; set; } = new List<HolidayType>();
}

public enum HolidayType
{
    Public,
    School
}