using System.Security.Cryptography;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SpotifyClone.Playlists.Application.Features.Playlists.Commands.AddTrack;
using SpotifyClone.Playlists.Application.Features.Playlists.Commands.Create;
using SpotifyClone.Playlists.Infrastructure.Persistence.Database;

namespace SpotifyClone.Playlists.Infrastructure.Persistence.Initialization;

public static class PlaylistsSeeder
{
    private const int TrackCount = 24;

    public static IList<Guid> Playlists { get; } = [];

    public static async Task SeedPlaylistsAsync(
        PlaylistsAppDbContext context,
        ISender sender)
    {
        for (int i = 0;
             await context.TrackReferences.CountAsync() != TrackCount
             && await context.UserReferences.AnyAsync()
             && i < 30;
             i++)
        {
            await Task.Delay(10_000);
        }

        if (await context.TrackReferences.CountAsync() != TrackCount ||
            !await context.UserReferences.AnyAsync())
        {
            return;
        }

        for (int i = 0; i < 5; i++)
        {
            Playlists.Add((await sender.Send(new CreatePlaylistCommand())).Value.PlaylistId);
        }

        List<Guid> trackIds = await context.TrackReferences
            .Select(t => t.Id)
            .ToListAsync();

        foreach (Guid playlistId in Playlists)
        {
            int tracksToTake = RandomNumberGenerator.GetInt32(5, 13);

            IEnumerable<Guid> tracksForThisPlaylist = trackIds
                .OrderBy(_ => RandomNumberGenerator.GetInt32(int.MaxValue))
                .Take(tracksToTake);

            foreach (Guid trackId in tracksForThisPlaylist)
            {
                await sender.Send(new AddTrackToPlaylistCommand(playlistId, trackId));
            }
        }
    }
}
