using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using CaseStudy.Core.Common;
using CaseStudy.Core.Entities;
using CaseStudy.Shared.Services;
using System.Reflection;

namespace CaseStudy.DataAccess.Persistence;

public class DatabaseContext : DbContext /*IdentityDbContext<ApplicationUser, IdentityRole<int>, int>*/
{
    private readonly IClaimService _claimService;

    public DatabaseContext(DbContextOptions<DatabaseContext> options, IClaimService claimService) : base(options)
    {
        _claimService = claimService;
    }

    // Migration add command
    // dotnet tool install --global dotnet-ef
    // Migration add command
    //dotnet ef migrations add Migration-Name --project CaseStudy.DataAccess -o Persistence/Migrations --startup-project CaseStudy.API
    // Migration update command
    // dotnet ef database update --project CaseStudy.DataAccess --startup-project CaseStudy.API

    public DbSet<User> Users { get; set; }
    public DbSet<IdentityRole<int>> Roles { get; set; }
    public DbSet<IdentityUserRole<int>> UserRoles { get; set; }
    public DbSet<IdentityUserClaim<int>> UserClaims { get; set; }
    public DbSet<IdentityUserLogin<int>> UserLogins { get; set; }
    public DbSet<IdentityRoleClaim<int>> RoleClaims { get; set; }
    public DbSet<IdentityUserToken<int>> UserTokens { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        builder.Entity<IdentityRole<int>>(entity => entity.ToTable("roles"));
        builder.Entity<IdentityUserRole<int>>(entity =>
        {
            entity.ToTable("user_roles");
            entity.HasKey(iur => new { iur.UserId, iur.RoleId });
        });
        builder.Entity<IdentityUserClaim<int>>(entity => entity.ToTable("user_claims"));
        builder.Entity<IdentityUserLogin<int>>(entity =>
        {
            entity.ToTable("user_logins");
            entity.HasKey(login => new { login.LoginProvider, login.ProviderKey });
        });
        builder.Entity<IdentityRoleClaim<int>>(entity => entity.ToTable("role_claims"));
        builder.Entity<IdentityUserToken<int>>(entity =>
        {
            entity.ToTable("user_tokens");
            entity.HasKey(e => new { e.UserId, e.LoginProvider, e.Name });
        });
    }

    public new async Task<int> SaveChangesAsync(CancellationToken cancellationToken = new())
    {
        foreach (var entry in ChangeTracker.Entries<IAuditedEntity>())
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedBy = Guid.Parse(_claimService.GetUserId());
                    entry.Entity.CreatedOn = DateTime.Now;
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedBy = Guid.Parse(_claimService.GetUserId());
                    entry.Entity.UpdatedOn = DateTime.Now;
                    break;
            }

        return await base.SaveChangesAsync(cancellationToken);
    }

    public override int SaveChanges()
    {
        // Look at the changed state of the entities and update the audit fields
        foreach (var entry in ChangeTracker.Entries<IAuditedEntity>())
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedBy = Guid.Parse(_claimService.GetUserId());
                    entry.Entity.CreatedOn = DateTime.Now;
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedBy = Guid.Parse(_claimService.GetUserId());
                    entry.Entity.UpdatedOn = DateTime.Now;
                    break;
            }

        return base.SaveChanges();
    }
}