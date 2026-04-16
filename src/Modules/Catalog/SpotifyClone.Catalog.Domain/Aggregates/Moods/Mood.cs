using SpotifyClone.Catalog.Domain.Aggregates.Moods.Events;
using SpotifyClone.Catalog.Domain.Aggregates.Moods.Rules;
using SpotifyClone.Catalog.Domain.Aggregates.Moods.ValueObjects;
using SpotifyClone.Shared.BuildingBlocks.Domain.Primitives;

namespace SpotifyClone.Catalog.Domain.Aggregates.Moods;

public sealed class Mood : AggregateRoot<MoodId, Guid>
{
    public string Name { get; private set; } = null!;
    public MoodCoverImage? Cover { get; private set; }

    public static Mood Create(MoodId id, string name)
    {
        ArgumentNullException.ThrowIfNull(id);

        MoodNameRules.Validate(name);

        var mood = new Mood(id, name, null);

        mood.RaiseDomainEvent(new MoodCreatedDomainEvent(mood.Id, mood.Name));

        return mood;
    }

    public void LinkNewCover(MoodCoverImage cover)
    {
        ArgumentNullException.ThrowIfNull(cover);

        TryUnlinkCover();

        Cover = cover;
        RaiseDomainEvent(new MoodLinkedToCoverImageDomainEvent(Id, Cover.ImageId));
    }

    public void TryUnlinkCover()
    {
        if (Cover is null)
        {
            return;
        }

        RaiseDomainEvent(new MoodUnlinkedFromCoverImageDomainEvent(Id, Cover.ImageId));
        Cover = null;
    }

    public void Rename(string name)
    {
        MoodNameRules.Validate(name);
        Name = name;

        RaiseDomainEvent(new MoodRenamedDomainEvent(Id, Name));
    }

    public void PrepareForDeletion()
    {
        TryUnlinkCover();
        RaiseDomainEvent(new MoodDeletedDomainEvent(Id));
    }

    private Mood(MoodId id, string name, MoodCoverImage? cover)
        : base(id)
    {
        Name = name;
        Cover = cover;
    }

    private Mood()
    {
    }
}
