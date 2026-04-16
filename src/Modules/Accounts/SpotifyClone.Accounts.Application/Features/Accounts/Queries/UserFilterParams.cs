namespace SpotifyClone.Accounts.Application.Features.Accounts.Queries;

public sealed record UserFilterParams(
    string? DisplayName = null,
    string? Gender = null);
