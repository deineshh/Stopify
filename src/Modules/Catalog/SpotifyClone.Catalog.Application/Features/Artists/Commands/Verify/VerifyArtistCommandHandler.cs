using SpotifyClone.Catalog.Application.Abstractions;
using SpotifyClone.Catalog.Application.Errors;
using SpotifyClone.Catalog.Domain.Aggregates.Artists;
using SpotifyClone.Catalog.Domain.Aggregates.Artists.ValueObjects;
using SpotifyClone.Shared.BuildingBlocks.Application.Abstractions.Commands;
using SpotifyClone.Shared.BuildingBlocks.Application.Results;

namespace SpotifyClone.Catalog.Application.Features.Artists.Commands.Verify;

internal sealed class VerifyArtistCommandHandler(
    ICatalogUnitOfWork unit)
    : ICommandHandler<VerifyArtistCommand, VerifyArtistCommandResult>
{
    private readonly ICatalogUnitOfWork _unit = unit;

    public async Task<Result<VerifyArtistCommandResult>> Handle(
        VerifyArtistCommand request,
        CancellationToken cancellationToken)
    {
        Artist? artist = await _unit.Artists.GetByIdAsync(
            ArtistId.From(request.ArtistId),
            cancellationToken);
        if (artist is null)
        {
            return Result.Failure<VerifyArtistCommandResult>(ArtistErrors.NotFound);
        }

        artist.Verify();

        return new VerifyArtistCommandResult();
    }
}
