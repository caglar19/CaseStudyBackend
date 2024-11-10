namespace CaseStudy.Application.Models.Holiday;

public class Subdivision
{
    public required string Code { get; set; }
    public required string IsoCode { get; set; }
    public required string ShortName { get; set; }
    public required ICollection<Name> Name { get; set; }
}

public class Name
{
    public required string Language { get; set; }
    public required string Text { get; set; }
}

public class SubdivisionResponseModel
{
    public Guid Id { get; set; }
    public required string Code { get; set; }
    public required string IsoCode { get; set; }
    public required string ShortName { get; set; }
    public required string LongName { get; set; }
}