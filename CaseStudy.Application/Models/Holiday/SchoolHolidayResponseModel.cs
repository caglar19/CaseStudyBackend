namespace CaseStudy.Application.Models.Holiday;

public class SchoolHolidayResponseModel
{
    public string Id { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Type { get; set; } // "School"
    public List<HolidayName> Name { get; set; }
    public string RegionalScope { get; set; } // "Regional"
    public string TemporalScope { get; set; } // "FullDay"
    public bool Nationwide { get; set; }
    public List<Subdivision> Subdivisions { get; set; }
}
