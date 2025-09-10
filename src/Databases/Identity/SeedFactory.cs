using Microsoft.AspNetCore.Identity;

namespace Journal.Databases.Identity;

public class SeedFactory
{
    public async Task SeedAdmins(IdentityContext context)
    {
        var id = "fdfa4136-ada3-41dc-b16e-8fd9556d4574";
        var password = "NewPassword@1";

        var testAdmin = new IdentityUser()
        {
            Id = id,
            UserName = "systemtester",
            Email = "systemtester@journal.com",
            NormalizedEmail = "SYSTEMTESTER@JOURNAL.COM",
            EmailConfirmed = true,
            PhoneNumber = "0564330462",
            PhoneNumberConfirmed = true,
            SecurityStamp = Guid.NewGuid().ToString(),
            ConcurrencyStamp = Guid.NewGuid().ToString()
        };

        var hasher = new PasswordHasher<IdentityUser>();
        testAdmin.PasswordHash = hasher.HashPassword(testAdmin, password);

        context.Users.Add(testAdmin);
        await context.SaveChangesAsync();
    }
}
