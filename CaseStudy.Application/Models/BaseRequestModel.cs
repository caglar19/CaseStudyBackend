namespace CaseStudy.Application.Models;

public abstract class BaseRequestModel
{
    public int? PageIndex { get; set; }
    public int? PageSize { get; set; }
    public string? SortField { get; set; }
    public string? SortOrder { get; set; }
    public string? Search { get; set; }
}