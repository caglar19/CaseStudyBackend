namespace CaseStudy.Application.Models.Holiday;

public class CountryName
{
    public required string Language { get; set; }
    public required string Text { get; set; }
}

public class CountryHoliday
{
    public required string IsoCode { get; set; }
    public required ICollection<CountryName> Name { get; set; }
}

public class CountryModel
{
    public Guid Id { get; set; }
    public required string IsoCode { get; set; }
    public required string? Name { get; set; }
}
