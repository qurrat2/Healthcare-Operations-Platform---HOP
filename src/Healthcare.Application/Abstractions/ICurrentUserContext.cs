namespace Healthcare.Application.Abstractions;

public interface ICurrentUserContext
{
    long? UserId { get; }
    string? Username { get; }
    string? Role { get; }
}
