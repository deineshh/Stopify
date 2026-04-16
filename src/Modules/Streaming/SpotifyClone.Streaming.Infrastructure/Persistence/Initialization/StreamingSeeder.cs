using MediatR;
using SpotifyClone.Streaming.Application.Features.Media.Commands.UploadAudioAsset;

namespace SpotifyClone.Streaming.Infrastructure.Persistence.Initialization;

public static class StreamingSeeder
{
    public static async Task SeedAudioAssetsAsync(
        IEnumerable<Guid> trackIds,
        ISender sender)
    {
        List<Guid> trackList = [.. trackIds];

        if (trackList.Count <= 0)
        {
            return;
        }

        string directory = Path.Combine(AppContext.BaseDirectory, "Persistence", "Initialization", "AudioFiles");
        string[] audioFiles = Directory.GetFiles(directory, "*.mp3");

        for (int i = 0; i < trackList.Count; i++)
        {
            if (i == 5 || i == 10) // Skip some tracks to simulate tracks without audio
            {
                continue;
            }

            using FileStream stream = File.OpenRead(audioFiles[
                i == 0 ? i : i % audioFiles.Length]);
            await sender.Send(new UploadAudioAssetCommand($"{i+1}.mp3", stream, trackList[i]));
        }
    }
}
