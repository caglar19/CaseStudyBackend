using System.ComponentModel.DataAnnotations;
using CaseStudy.Core.Enums;

namespace CaseStudy.Core.Common;

public abstract class BaseEntity
{
    [Key]
    [Required]
    public int Id { get; set; }
    [Required]
    public Guid RefId { get; set; }
    [Required]
    public Guid CreatedBy { get; set; }
    [Required]
    public DateTime CreatedOn { get; set; }
    public Guid? UpdatedBy { get; set; }
    public DateTime? UpdatedOn { get; set; }
    public Guid? DeletedBy { get; set; }
    public DateTime? DeletedOn { get; set; }
    [Required]
    public EDataStatus DataStatus { get; set; } = EDataStatus.Active;
}