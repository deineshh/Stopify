using MediatR;
using Microsoft.EntityFrameworkCore;
using SpotifyClone.Catalog.Application.Features.Albums.Commands.Create;
using SpotifyClone.Catalog.Application.Features.Albums.Commands.PublishAlbum;
using SpotifyClone.Catalog.Application.Features.Artists.Commands.Create;
using SpotifyClone.Catalog.Application.Features.Genres.Commands.Create;
using SpotifyClone.Catalog.Application.Features.Moods.Commands.Create;
using SpotifyClone.Catalog.Application.Features.Tracks.Commands.Create;
using SpotifyClone.Catalog.Domain.Aggregates.Tracks.Enums;
using SpotifyClone.Catalog.Infrastructure.Persistence.Database;

namespace SpotifyClone.Catalog.Infrastructure.Persistence.Initialization;

public static class CatalogSeeder
{
    public static IList<Guid> Genres { get; } = [];
    public static IList<Guid> Moods { get; } = [];
    public static IList<Guid> Artists { get; } = [];
    public static IList<Guid> Albums { get; } = [];
    public static IList<Guid> Tracks { get; } = [];

    public static async Task SeedGenresAsync(
        CatalogAppDbContext context,
        ISender sender)
    {
        if (await context.Genres.AnyAsync())
        {
            return;
        }

        Genres.Add((await sender.Send(new CreateGenreCommand("Classical"))).Value.GenreId);
        Genres.Add((await sender.Send(new CreateGenreCommand("Pop"))).Value.GenreId);
        Genres.Add((await sender.Send(new CreateGenreCommand("Rock"))).Value.GenreId);
        Genres.Add((await sender.Send(new CreateGenreCommand("Jazz"))).Value.GenreId);
        Genres.Add((await sender.Send(new CreateGenreCommand("Hip-Hop"))).Value.GenreId);
    }

    public static async Task SeedMoodsAsync(
        CatalogAppDbContext context,
        ISender sender)
    {
        if (await context.Moods.AnyAsync())
        {
            return;
        }

        Moods.Add((await sender.Send(new CreateMoodCommand("Calm"))).Value.MoodId);
        Moods.Add((await sender.Send(new CreateMoodCommand("Focus"))).Value.MoodId);
        Moods.Add((await sender.Send(new CreateMoodCommand("Happy"))).Value.MoodId);
        Moods.Add((await sender.Send(new CreateMoodCommand("Sad"))).Value.MoodId);
        Moods.Add((await sender.Send(new CreateMoodCommand("Energetic"))).Value.MoodId);
    }

    public static async Task SeedArtistsAsync(
        CatalogAppDbContext context,
        ISender sender)
    {
        if (await context.Artists.AnyAsync())
        {
            return;
        }

        Artists.Add((await sender.Send(new CreateArtistCommand("Azahriah"))).Value.ArtistId);
        Artists.Add((await sender.Send(new CreateArtistCommand("DESH"))).Value.ArtistId);
        Artists.Add((await sender.Send(new CreateArtistCommand("YoungFly"))).Value.ArtistId);
        Artists.Add((await sender.Send(new CreateArtistCommand("Manuel"))).Value.ArtistId);
        Artists.Add((await sender.Send(new CreateArtistCommand("Kesha"))).Value.ArtistId);
    }

    public static async Task SeedAlbumsAsync(
        CatalogAppDbContext context,
        ISender sender)
    {
        if (!Artists.Any() || await context.Albums.AnyAsync())
        {
            return;
        }

        Albums.Add((await sender.Send(new CreateAlbumCommand(
            "a lo tuloldalan", [Artists[0], Artists[1]]))).Value.AlbumId);
        Albums.Add((await sender.Send(new CreateAlbumCommand(
            "ret", [Artists[0], Artists[1]]))).Value.AlbumId);
        Albums.Add((await sender.Send(new CreateAlbumCommand(
            "TIP TIP", Artists))).Value.AlbumId);
        Albums.Add((await sender.Send(new CreateAlbumCommand(
            "skatulya", [Artists[0]]))).Value.AlbumId);
        Albums.Add((await sender.Send(new CreateAlbumCommand(
            "tripq", [Artists[1]]))).Value.AlbumId);
    }

    public static async Task SeedTracksAsync(
        CatalogAppDbContext context,
        ISender sender)
    {
        if (!Albums.Any() || !Artists.Any() || !Genres.Any() || !Moods.Any() || await context.Tracks.AnyAsync())
        {
            return;
        }

        // 1. Популярний трек з декількома артистами та Explicit контентом
        Tracks.Add((await sender.Send(new CreateTrackCommand(
            "Intro",
            true,
            Albums[3],
            [Artists[0]],
            [],
            [Genres[4]],
            [Moods[4]]))).Value.TrackId);

        // 2. Трек з фітом (Main + Featured)
        Tracks.Add((await sender.Send(new CreateTrackCommand(
            "Pannonia",
            false,
            Albums[0],
            [Artists[0]],
            [Artists[1]],
            [Genres[1], Genres[4]],
            [Moods[2]]))).Value.TrackId);

        // 3. Довга назва для перевірки UI-верстки
        Tracks.Add((await sender.Send(new CreateTrackCommand(
            "Very Long Track Title for Testing Purposes That Might Break Your CSS Layout If Not Handled Correctly",
            false,
            Albums[2],
            [Artists[2]],
            [Artists[3], Artists[4]],
            [Genres[2]],
            [Moods[4]]))).Value.TrackId);

        // 4. Трек для фокусу (декілька настроїв, без Explicit)
        Tracks.Add((await sender.Send(new CreateTrackCommand(
            "Deep Focus Session",
            false,
            Albums[4],
            [Artists[1]],
            [],
            [Genres[0], Genres[3]],
            [Moods[1], Moods[0]]))).Value.TrackId);

        // 5. Гіп-хоп трек з великою кількістю метаданих
        Tracks.Add((await sender.Send(new CreateTrackCommand(
            "Ret",
            true,
            Albums[1],
            [Artists[0], Artists[1]],
            [Artists[2]],
            [Genres[4]],
            [Moods[2], Moods[4]]))).Value.TrackId);

        // 6-25. Генеримо пачку однотипних треків для тестування пагінації (Pagination Test)
        // Це дозволить тобі побачити перехід між сторінками
        for (int i = 1; i <= 20; i++)
        {
            Tracks.Add((await sender.Send(new CreateTrackCommand(
                $"Track Number {i}",
                i % 5 == 0, // Кожен п'ятий буде Explicit
                Albums[i % Albums.Count],
                [Artists[i % Artists.Count]],
                [],
                [Genres[i % Genres.Count]],
                [Moods[i % Moods.Count]]))).Value.TrackId);
        }

        // 26. Трек з порожніми списками (Edge Case)
        Tracks.Add((await sender.Send(new CreateTrackCommand(
            "Silence is Golden",
            false,
            Albums[0],
            [Artists[0]],
            [],
            [Genres[0]],
            [Moods[0]]))).Value.TrackId);
    }

    public static async Task PublishSeededAlbumsAsync(
        CatalogAppDbContext context,
        ISender sender)
    {
        if (!Albums.Any() || !Artists.Any() || !Genres.Any() || !Moods.Any() || !Tracks.Any())
        {
            return;
        }

        for (int i = 0;
             await context.Tracks.CountAsync(t => t.Status == TrackStatus.Published) != 24
             && i < 30;
             i++)
        {
            await Task.Delay(10_000);
        }

        if (await context.Tracks.CountAsync(t => t.Status == TrackStatus.Published) != 24)
        {
            return;
        }

        foreach (Guid albumId in Albums)
        {
            await sender.Send(new PublishAlbumCommand(albumId, DateTimeOffset.UtcNow));
        }
    }
}
