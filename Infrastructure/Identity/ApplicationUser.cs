using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Identity;

// We extend IdentityUser to add our own custom fields.
// IdentityUser already gives us: Id, UserName, Email, PasswordHash, etc.
public class ApplicationUser : IdentityUser
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}";
}
