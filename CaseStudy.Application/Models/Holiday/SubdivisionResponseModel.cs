namespace CaseStudy.Application.Models.Holiday;

public class Subdivision
{
    public string Code { get; set; }
    public string IsoCode { get; set; }
    public string ShortName { get; set; }
    public List<Category> Category { get; set; }
    public List<Name> Name { get; set; }
    public List<string> OfficialLanguages { get; set; }
    public List<Subdivision> Children { get; set; } // Eğer varsa alt bölge alt bölümleri
}

public class Category
{
    public string Language { get; set; }
    public string Text { get; set; }
}

public class Name
{
    public string Language { get; set; }
    public string Text { get; set; }
}
