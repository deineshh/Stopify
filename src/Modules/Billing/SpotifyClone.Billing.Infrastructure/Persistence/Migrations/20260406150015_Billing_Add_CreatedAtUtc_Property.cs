using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SpotifyClone.Billing.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class Billing_Add_CreatedAtUtc_Property : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
        => migrationBuilder.AddColumn<DateTimeOffset>(
            name: "created_at_utc",
            schema: "billing",
            table: "subscriptions",
            type: "timestamp with time zone",
            nullable: false,
            defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
        => migrationBuilder.DropColumn(
            name: "created_at_utc",
            schema: "billing",
            table: "subscriptions");
}
