using FluentValidation;

namespace SpotifyClone.Catalog.Application.Features.Tracks.Queries.ListByIds;

public sealed class ListTracksByIdsQueryValidator
    : AbstractValidator<ListTracksByIdsQuery>
{
    public ListTracksByIdsQueryValidator()
        => RuleFor(x => x.TrackIds)
            .NotEmpty().WithMessage("Track IDs are required.");
}
