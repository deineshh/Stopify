using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SpotifyClone.Streaming.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class Streaming_Add_CreatedAtUtc_Property : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<DateTimeOffset>(
            name: "created_at_utc",
            schema: "streaming",
            table: "playback_history_entries",
            type: "timestamp with time zone",
            nullable: false,
            defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

        migrationBuilder.AddColumn<DateTimeOffset>(
            name: "created_at_utc",
            schema: "streaming",
            table: "image_assets",
            type: "timestamp with time zone",
            nullable: false,
            defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

        migrationBuilder.AddColumn<DateTimeOffset>(
            name: "created_at_utc",
            schema: "streaming",
            table: "audio_assets",
            type: "timestamp with time zone",
            nullable: false,
            defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "created_at_utc",
            schema: "streaming",
            table: "playback_history_entries");

        migrationBuilder.DropColumn(
            name: "created_at_utc",
            schema: "streaming",
            table: "image_assets");

        migrationBuilder.DropColumn(
            name: "created_at_utc",
            schema: "streaming",
            table: "audio_assets");
    }
}
