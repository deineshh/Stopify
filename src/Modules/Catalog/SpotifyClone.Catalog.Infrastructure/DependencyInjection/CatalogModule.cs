using FluentValidation;
using Hangfire;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SpotifyClone.Catalog.Application;
using SpotifyClone.Catalog.Application.Abstractions;
using SpotifyClone.Catalog.Application.Abstractions.Data;
using SpotifyClone.Catalog.Application.Abstractions.Repositories;
using SpotifyClone.Catalog.Application.Behaviors;
using SpotifyClone.Catalog.Application.Errors;
using SpotifyClone.Catalog.Application.Jobs;
using SpotifyClone.Catalog.Domain.Aggregates.Albums;
using SpotifyClone.Catalog.Domain.Aggregates.Artists;
using SpotifyClone.Catalog.Domain.Aggregates.Genres;
using SpotifyClone.Catalog.Domain.Aggregates.Moods;
using SpotifyClone.Catalog.Domain.Aggregates.Tracks;
using SpotifyClone.Catalog.Domain.Services;
using SpotifyClone.Catalog.Infrastructure.Persistence;
using SpotifyClone.Catalog.Infrastructure.Persistence.Database;
using SpotifyClone.Catalog.Infrastructure.Persistence.Initialization;
using SpotifyClone.Catalog.Infrastructure.Persistence.Queries;
using SpotifyClone.Catalog.Infrastructure.Persistence.Repositories;
using SpotifyClone.Shared.BuildingBlocks.Application.Abstractions;
using SpotifyClone.Shared.BuildingBlocks.Application.Abstractions.Mappers;

namespace SpotifyClone.Catalog.Infrastructure.DependencyInjection;

public static class CatalogModule
{
    public static IServiceCollection AddCatalogModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(
            CatalogApplicationAssemblyReference.Assembly));

        services.AddValidatorsFromAssembly(CatalogApplicationAssemblyReference.Assembly);

        services.AddDbContext<CatalogAppDbContext>(options => options.UseNpgsql(
            configuration.GetConnectionString("MainDb"),
            b => b.MigrationsAssembly(typeof(CatalogAppDbContext).Assembly.FullName)));

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<ICatalogUnitOfWork>());
        services.AddScoped<ICatalogUnitOfWork, CatalogEfCoreUnitOfWork>();

        services.AddScoped<ITrackRepository, TrackEfCoreRepository>();
        services.AddScoped<IAlbumRepository, AlbumEfCoreRepository>();
        services.AddScoped<IArtistRepository, ArtistEfCoreRepository>();
        services.AddScoped<IGenreRepository, GenreEfCoreRepository>();
        services.AddScoped<IMoodRepository, MoodEfCoreRepository>();
        services.AddScoped<IOutboxRepository, OutboxEfCoreRepository>();

        services.AddScoped<AlbumTrackDomainService>();
        services.AddScoped<ITrackReadService, TrackEfCoreReadService>();
        services.AddScoped<IAlbumReadService, AlbumEfCoreReadService>();
        services.AddScoped<IArtistReadService, ArtistEfCoreReadService>();
        services.AddScoped<IGenreReadService, GenreEfCoreReadService>();
        services.AddScoped<IMoodReadService, MoodEfCoreReadService>();

        services.AddScoped<IDomainExceptionMapper, CatalogDomainExceptionMapper>();

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(CatalogTransactionalPipelineBehavior<,>));
        services.AddTransient<LinkTrackToAudioFileJob>();
        services.AddTransient<MarkTrackAsReadyToPublishJob>();
        services.AddTransient<ProcessOutboxMessagesJob>();

        return services;
    }

    public static async Task UseCatalogModule(this IApplicationBuilder app)
    {
        using IServiceScope scope = app.ApplicationServices.CreateScope();
        CatalogAppDbContext dbContext = scope.ServiceProvider
            .GetRequiredService<CatalogAppDbContext>();
        ISender sender = scope.ServiceProvider
            .GetRequiredService<ISender>();

        await CatalogSeeder.SeedGenresAsync(dbContext, sender);
        await CatalogSeeder.SeedMoodsAsync(dbContext, sender);
        await CatalogSeeder.SeedArtistsAsync(dbContext, sender);
        await CatalogSeeder.SeedAlbumsAsync(dbContext, sender);
        await CatalogSeeder.SeedTracksAsync(dbContext, sender);
        //await CatalogSeeder.PublishSeededAlbumsAsync(dbContext, sender);

        IRecurringJobManager recurringJobManager =
            scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();
        recurringJobManager.AddOrUpdate<ProcessOutboxMessagesJob>(
            "catalog-outbox-processor",
            job => job.ProcessAsync(),
            "*/5 * * * * *" // Every 5 seconds (Cron expression)
        );
    }
}
