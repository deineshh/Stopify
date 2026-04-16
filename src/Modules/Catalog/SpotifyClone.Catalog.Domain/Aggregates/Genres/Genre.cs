using SpotifyClone.Catalog.Domain.Aggregates.Genres.Events;
using SpotifyClone.Catalog.Domain.Aggregates.Genres.Rules;
using SpotifyClone.Catalog.Domain.Aggregates.Genres.ValueObjects;
using SpotifyClone.Shared.BuildingBlocks.Domain.Primitives;

namespace SpotifyClone.Catalog.Domain.Aggregates.Genres;

public sealed class Genre : AggregateRoot<GenreId, Guid>
{
    public string Name { get; private set; } = null!;
    public GenreCoverImage? Cover { get; private set; }

    public static Genre Create(GenreId id, string name)
    {
        ArgumentNullException.ThrowIfNull(id);

        GenreNameRules.Validate(name);

        var genre = new Genre(id, name, null);

        genre.RaiseDomainEvent(new GenreCreatedDomainEvent(genre.Id, genre.Name));

        return genre;
    }

    public void LinkNewCover(GenreCoverImage cover)
    {
        ArgumentNullException.ThrowIfNull(cover);

        TryUnlinkCover();

        Cover = cover;
        RaiseDomainEvent(new GenreLinkedToCoverImageDomainEvent(Id, Cover.ImageId));
    }

    public void TryUnlinkCover()
    {
        if (Cover is null)
        {
            return;
        }

        RaiseDomainEvent(new GenreUnlinkedFromCoverImageDomainEvent(Id, Cover.ImageId));
        Cover = null;
    }

    public void Rename(string name)
    {
        GenreNameRules.Validate(name);
        Name = name;

        RaiseDomainEvent(new GenreRenamedDomainEvent(Id, Name));
    }

    public void PrepareForDeletion()
    {
        TryUnlinkCover();
        RaiseDomainEvent(new GenreDeletedDomainEvent(Id));
    }

    private Genre(GenreId id, string name, GenreCoverImage? cover)
        : base(id)
    {
        Name = name;
        Cover = cover;
    }

    private Genre()
    {
    }
}
