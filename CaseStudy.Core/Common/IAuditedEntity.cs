using CaseStudy.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace CaseStudy.Core.Common;

public interface IAuditedEntity
{
    [Key]
    public int Id { get; set; }
    public Guid RefId { get; set; }
    public Guid CreatedBy { get; set; }
    public DateTime CreatedOn { get; set; }
    public Guid? UpdatedBy { get; set; }
    public DateTime? UpdatedOn { get; set; }
    public Guid? DeletedBy { get; set; }
    public DateTime? DeletedOn { get; set; }
    public EDataStatus DataStatus { get; set; }
}