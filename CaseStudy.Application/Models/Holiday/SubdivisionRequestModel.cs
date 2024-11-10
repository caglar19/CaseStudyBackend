namespace CaseStudy.Application.Models.Holiday;

public class SubdivisionRequestModel
{
    public required string CountryIsoCode { get; set; }
    public string? LanguageIsoCode { get; set; }
}