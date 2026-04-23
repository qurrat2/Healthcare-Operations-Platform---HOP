using Healthcare.Domain.Entities;

namespace Healthcare.Application.Abstractions.Security;

public interface IJwtTokenGenerator
{
    string GenerateToken(User user, string role);
}
