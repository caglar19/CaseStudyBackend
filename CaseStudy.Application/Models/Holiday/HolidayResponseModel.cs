namespace CaseStudy.Application.Models.Holiday;

public class HolidayResponseModel
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
