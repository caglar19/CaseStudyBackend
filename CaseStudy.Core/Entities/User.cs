using Microsoft.AspNetCore.Identity;
using CaseStudy.Core.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace CaseStudy.Core.Entities;

[Table("users")]
public class User : IdentityUser<int>
{
    public Guid RefId { get; set; }
    public Guid CreatedBy { get; set; }
    public DateTime CreatedOn { get; set; }
    public Guid? UpdatedBy { get; set; }
    public DateTime? UpdatedOn { get; set; }
    public Guid? DeletedBy { get; set; }
    public DateTime? DeletedOn { get; set; }
    public EDataStatus DataStatus { get; set; }
}