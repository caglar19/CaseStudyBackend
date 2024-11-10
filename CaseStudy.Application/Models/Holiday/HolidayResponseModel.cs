namespace CaseStudy.Application.Models.Holiday;

public class Holiday
{
    public string Id { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Type { get; set; }
    public List<HolidayName> Name { get; set; }
    public string RegionalScope { get; set; }
    public string TemporalScope { get; set; }
    public bool Nationwide { get; set; }
}

public class HolidayName
{
    public string Language { get; set; }
    public string Text { get; set; }
}

public class HolidayResponseModel
{
    public required Guid Id { get; set; }
    public required string StartDate { get; set; }
    public required string EndDate { get; set; }
    public string? Name { get; set; }
}