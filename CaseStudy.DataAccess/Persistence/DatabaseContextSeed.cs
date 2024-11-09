using Microsoft.AspNetCore.Identity;
using CaseStudy.Core.Entities;

namespace CaseStudy.DataAccess.Persistence;

public static class DatabaseContextSeed
{
    public static async Task SeedDatabaseAsync(DatabaseContext context,
        UserManager<User> userManager)
    {
        if (!userManager.Users.Any())
        {
            var user = new User {
                UserName = "admin",
                Email = "admin@admin.com",
                EmailConfirmed = true,
                RefId = Guid.NewGuid(),
            };

            user.CreatedBy = user.RefId;
            user.CreatedOn = DateTime.Now;

            await userManager.CreateAsync(user, "Admin123.?");
            
            // var vendor = new VendorCreateModel {
            //     Name = "CaseStudy",
            //     Description = "CaseStudy is a platform for online holidays.",
            //     CreatedBy = user.RefId,
            //     CreatedOn = DateTime.Now,
            // };
            //
            // await vendorService.CreateAsync(vendor);
            //
            // var category = new CategoryCreateModel() {
            //     Name = "Programming",
            //     Description = "Programming holidays",
            //     CreatedBy = user.RefId,
            //     CreatedOn = DateTime.Now,
            // };
            //
            // await categoryService.CreateAsync(category);
        }

        await context.SaveChangesAsync();
    }
}