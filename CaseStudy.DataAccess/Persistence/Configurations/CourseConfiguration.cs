using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using CaseStudy.Core.Entities;

namespace CaseStudy.DataAccess.Persistence.Configurations;

internal class HolidayConfiguration : IEntityTypeConfiguration<Holiday>
{
    public void Configure(EntityTypeBuilder<Holiday> builder)
    {
        // Configure the properties of the Holiday entity
        builder.Property(e => e.Id).IsRequired().ValueGeneratedOnAdd();
        builder.Property(e => e.RefId).IsRequired().ValueGeneratedOnAdd();
        builder.Property(e => e.CreatedBy).IsRequired().ValueGeneratedOnAdd();
        builder.Property(e => e.CreatedOn).IsRequired().ValueGeneratedOnAdd();
        
    }
}
