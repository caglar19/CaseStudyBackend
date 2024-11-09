using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using CaseStudy.Shared.Services;
using System.Reflection;

namespace CaseStudy.DataAccess.Persistence;

public class DatabaseContext : DbContext 
{
    private readonly IClaimService _claimService;

    public DatabaseContext(DbContextOptions<DatabaseContext> options, IClaimService claimService) : base(options)
    {
        _claimService = claimService;
    }

}