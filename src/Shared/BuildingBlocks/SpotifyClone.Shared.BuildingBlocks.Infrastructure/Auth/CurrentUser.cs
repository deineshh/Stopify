using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using SpotifyClone.Shared.BuildingBlocks.Application.Abstractions.Primitives;
using SpotifyClone.Shared.BuildingBlocks.Application.Auth;

namespace SpotifyClone.Shared.BuildingBlocks.Infrastructure.Auth;

internal sealed class CurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
    private static readonly AsyncLocal<ClaimsPrincipal?> _manualUser = new();

    private ClaimsPrincipal? User => _manualUser.Value ?? _httpContextAccessor.HttpContext?.User;

    public bool IsAuthenticated =>
        User?.Identity?.IsAuthenticated == true;

    public Guid Id
    {
        get
        {
            Claim? claim = User?.FindFirst(ClaimTypes.NameIdentifier)
                ?? throw new UnauthorizedAccessException("User is not authenticated.");

            return Guid.Parse(claim.Value);
        }
    }

    public string? Email
        => User?.FindFirst(ClaimTypes.Email)?.Value;

    public bool IsPremium
        => User?.HasClaim(c => c.Type == "subscription_level" && c.Value == "premium") ?? false;

    public bool IsInRole(string role)
    {
        if (User is null || !IsAuthenticated)
        {
            return false;
        }

        return User.IsInRole(role);
    }

    public void SetUser(Guid userId, string primaryRole, bool isPremium)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString())
        };

        string[] roles = UserRoles.CalculateBy(primaryRole);
        
        foreach (string role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        if (isPremium)
        {
            claims.Add(new Claim("subscription_level", "premium"));
        }

        var identity = new ClaimsIdentity(claims, "ManualAuth");
        _manualUser.Value = new ClaimsPrincipal(identity);
    }
}
