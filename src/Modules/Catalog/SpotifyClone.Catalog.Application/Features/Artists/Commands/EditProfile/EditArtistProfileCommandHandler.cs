using SpotifyClone.Catalog.Application.Abstractions;
using SpotifyClone.Catalog.Application.Errors;
using SpotifyClone.Catalog.Domain.Aggregates.Artists;
using SpotifyClone.Catalog.Domain.Aggregates.Artists.ValueObjects;
using SpotifyClone.Shared.BuildingBlocks.Application.Abstractions.Commands;
using SpotifyClone.Shared.BuildingBlocks.Application.Abstractions.Primitives;
using SpotifyClone.Shared.BuildingBlocks.Application.Auth;
using SpotifyClone.Shared.BuildingBlocks.Application.Results;

namespace SpotifyClone.Catalog.Application.Features.Artists.Commands.EditProfile;

internal sealed class EditArtistProfileCommandHandler(
    ICatalogUnitOfWork unit,
    ICurrentUser currentUser)
    : ICommandHandler<EditArtistProfileCommand, EditArtistProfileCommandResult>
{
    private readonly ICatalogUnitOfWork _unit = unit;
    private readonly ICurrentUser _currentUser = currentUser;

    public async Task<Result<EditArtistProfileCommandResult>> Handle(
        EditArtistProfileCommand request,
        CancellationToken cancellationToken)
    {
        Artist? artist = await _unit.Artists.GetByIdAsync(
            ArtistId.From(request.ArtistId),
            cancellationToken);
        if (artist is null)
        {
            return Result.Failure<EditArtistProfileCommandResult>(ArtistErrors.NotFound);
        }

        if ((!_currentUser.IsAuthenticated || _currentUser.Id != artist.OwnerId?.Value) &&
            !_currentUser.IsInRole(UserRoles.Admin))
        {
            return Result.Failure<EditArtistProfileCommandResult>(ArtistErrors.NotOwned);
        }

        artist.Rename(request.Name);

        if (request.Bio is not null)
        {
            artist.UpdateBio(request.Bio);
        }

        return new EditArtistProfileCommandResult();
    }
}
