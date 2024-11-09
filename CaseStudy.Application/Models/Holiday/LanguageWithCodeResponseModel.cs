namespace CaseStudy.Application.Models.Holiday;

public class LanguageName
{
    public string Language { get; set; }
    public string Text { get; set; }
}

public class Language
{
    public string IsoCode { get; set; }
    public List<LanguageName> Name { get; set; }
}

public class LanguageWithCode
{
    public string IsoCode { get; set; }
    public string Name { get; set; }
}
