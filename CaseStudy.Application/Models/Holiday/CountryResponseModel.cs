namespace CaseStudy.Application.Models.Holiday;

public class CountryName
{
    public string Language { get; set; }
    public string Text { get; set; }
}

public class Country
{
    public string IsoCode { get; set; }
    public List<CountryName> Name { get; set; }
    public List<string> OfficialLanguages { get; set; }
}
