namespace SpotifyClone.Shared.BuildingBlocks.Application.Abstractions.Primitives;

public interface ICurrentUser
{
    Guid Id { get; }
    string? Email { get; }
    bool IsPremium { get; }
    bool IsAuthenticated { get; }
    bool IsInRole(string role);
    void SetUser(Guid userId, string primaryRole, bool isPremium);
}
