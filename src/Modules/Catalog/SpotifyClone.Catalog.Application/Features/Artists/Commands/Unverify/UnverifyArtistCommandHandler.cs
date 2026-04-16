using SpotifyClone.Catalog.Application.Abstractions;
using SpotifyClone.Catalog.Application.Errors;
using SpotifyClone.Catalog.Domain.Aggregates.Artists;
using SpotifyClone.Catalog.Domain.Aggregates.Artists.ValueObjects;
using SpotifyClone.Shared.BuildingBlocks.Application.Abstractions.Commands;
using SpotifyClone.Shared.BuildingBlocks.Application.Results;

namespace SpotifyClone.Catalog.Application.Features.Artists.Commands.Unverify;

internal sealed class UnverifyArtistCommandHandler(
    ICatalogUnitOfWork unit)
    : ICommandHandler<UnverifyArtistCommand, UnverifyArtistCommandResult>
{
    private readonly ICatalogUnitOfWork _unit = unit;

    public async Task<Result<UnverifyArtistCommandResult>> Handle(
        UnverifyArtistCommand request,
        CancellationToken cancellationToken)
    {
        Artist? artist = await _unit.Artists.GetByIdAsync(
            ArtistId.From(request.ArtistId),
            cancellationToken);
        if (artist is null)
        {
            return Result.Failure<UnverifyArtistCommandResult>(ArtistErrors.NotFound);
        }

        artist.Unverify();

        return new UnverifyArtistCommandResult();
    }
}
