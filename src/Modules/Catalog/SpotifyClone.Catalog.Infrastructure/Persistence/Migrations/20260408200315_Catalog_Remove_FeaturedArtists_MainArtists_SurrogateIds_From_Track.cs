using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SpotifyClone.Catalog.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class Catalog_Remove_FeaturedArtists_MainArtists_SurrogateIds_From_Track : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropPrimaryKey(
            name: "PK_track_main_artists",
            schema: "catalog",
            table: "track_main_artists");

        migrationBuilder.DropPrimaryKey(
            name: "PK_track_featured_artists",
            schema: "catalog",
            table: "track_featured_artists");

        migrationBuilder.DropColumn(
            name: "id",
            schema: "catalog",
            table: "track_main_artists");

        migrationBuilder.DropColumn(
            name: "id",
            schema: "catalog",
            table: "track_featured_artists");

        migrationBuilder.AddPrimaryKey(
            name: "PK_track_main_artists",
            schema: "catalog",
            table: "track_main_artists",
            columns: new[] { "track_id", "artist_id" });

        migrationBuilder.AddPrimaryKey(
            name: "PK_track_featured_artists",
            schema: "catalog",
            table: "track_featured_artists",
            columns: new[] { "track_id", "artist_id" });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropPrimaryKey(
            name: "PK_track_main_artists",
            schema: "catalog",
            table: "track_main_artists");

        migrationBuilder.DropPrimaryKey(
            name: "PK_track_featured_artists",
            schema: "catalog",
            table: "track_featured_artists");

        migrationBuilder.AddColumn<Guid>(
            name: "id",
            schema: "catalog",
            table: "track_main_artists",
            type: "uuid",
            nullable: false,
            defaultValue: Guid.Empty);

        migrationBuilder.AddColumn<Guid>(
            name: "id",
            schema: "catalog",
            table: "track_featured_artists",
            type: "uuid",
            nullable: false,
            defaultValue: Guid.Empty);

        migrationBuilder.AddPrimaryKey(
            name: "PK_track_main_artists",
            schema: "catalog",
            table: "track_main_artists",
            column: "id");

        migrationBuilder.AddPrimaryKey(
            name: "PK_track_featured_artists",
            schema: "catalog",
            table: "track_featured_artists",
            column: "id");
    }
}
