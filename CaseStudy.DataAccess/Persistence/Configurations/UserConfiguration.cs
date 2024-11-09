using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using CaseStudy.Core.Entities;

namespace CaseStudy.DataAccess.Persistence.Configurations;

internal class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        // Configure the properties of the User entity
        builder.Property(e => e.Id).IsRequired().ValueGeneratedOnAdd();
        builder.Property(e => e.RefId).IsRequired().ValueGeneratedOnAdd();
        builder.Property(e => e.CreatedBy).IsRequired().ValueGeneratedOnAdd();
        builder.Property(e => e.CreatedOn).IsRequired().ValueGeneratedOnAdd();

        // Configure the relationships of the User entity
    }
}