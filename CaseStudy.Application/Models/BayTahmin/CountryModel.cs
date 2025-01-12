namespace CaseStudy.Application.Models.Holiday;

public class Paging
{
    public int current { get; set; }
    public int total { get; set; }
}

public class Country
{
    public string name { get; set; }
    public string code { get; set; }
    public string flag { get; set; }
}

public class CountryResponse
{
    public string get { get; set; }
    public List<object> parameters { get; set; }
    public List<object> errors { get; set; }
    public int results { get; set; }
    public Paging paging { get; set; }
    public List<CountryHoliday> response { get; set; }
}
