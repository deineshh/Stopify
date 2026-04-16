namespace SpotifyClone.Playlists.Application.Abstractions.Repositories;

public interface ITrackReferenceRepository
{
    Task<bool> ExistsAsync(
        Guid trackId,
        CancellationToken cancellationToken);

    Task<bool> ExistsAsync(
        IEnumerable<Guid> trackIds,
        CancellationToken cancellationToken);

    Task<bool> IsPublishedAsync(
        Guid trackId,
        CancellationToken cancellationToken);

    Task AddAsync(
        Guid trackId,
        CancellationToken cancellationToken = default);

    Task LinkCoverAsync(
        Guid trackId,
        Guid coverImageId,
        CancellationToken cancellationToken = default);

    Task UnlinkCoverAsync(
        Guid trackId,
        CancellationToken cancellationToken = default);

    Task MarkAsPublishedAsync(
        Guid trackId,
        CancellationToken cancellationToken = default);

    Task MarkAsNotPublishedAsync(
        Guid trackId,
        CancellationToken cancellationToken = default);

    Task MarkAsArchivedAsync(
        Guid trackId,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        Guid trackId,
        CancellationToken cancellationToken = default);
}
