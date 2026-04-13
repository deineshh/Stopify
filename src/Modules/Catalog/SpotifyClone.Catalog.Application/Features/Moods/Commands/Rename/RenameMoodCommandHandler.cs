using SpotifyClone.Catalog.Application.Abstractions;
using SpotifyClone.Catalog.Application.Errors;
using SpotifyClone.Catalog.Domain.Aggregates.Moods;
using SpotifyClone.Catalog.Domain.Aggregates.Moods.ValueObjects;
using SpotifyClone.Shared.BuildingBlocks.Application.Abstractions.Commands;
using SpotifyClone.Shared.BuildingBlocks.Application.Results;

namespace SpotifyClone.Catalog.Application.Features.Moods.Commands.Rename;

internal sealed class RenameMoodCommandHandler(
    ICatalogUnitOfWork unit)
    : ICommandHandler<RenameMoodCommand, RenameMoodCommandResult>
{
    private readonly ICatalogUnitOfWork _unit = unit;

    public async Task<Result<RenameMoodCommandResult>> Handle(
        RenameMoodCommand request,
        CancellationToken cancellationToken)
    {
        Mood? mood = await _unit.Moods.GetByIdAsync(
            MoodId.From(request.MoodId), cancellationToken);
        if (mood is null)
        {
            return Result.Failure<RenameMoodCommandResult>(MoodErrors.NotFound);
        }

        if (!await _unit.Moods.IsNameUniqueAsync(request.Name, cancellationToken))
        {
            return Result.Failure<RenameMoodCommandResult>(MoodErrors.InvalidName);
        }

        mood.Rename(request.Name);

        return new RenameMoodCommandResult();
    }
}
