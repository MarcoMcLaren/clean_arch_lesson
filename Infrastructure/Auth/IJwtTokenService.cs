using Infrastructure.Identity;

namespace Infrastructure.Auth;

public interface IJwtTokenService
{
    string GenerateToken(ApplicationUser user, IList<string> roles);
}
