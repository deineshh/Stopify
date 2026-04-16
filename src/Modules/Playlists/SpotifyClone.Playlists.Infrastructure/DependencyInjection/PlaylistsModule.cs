using FluentValidation;
using Hangfire;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SpotifyClone.Playlists.Application;
using SpotifyClone.Playlists.Application.Abstractions;
using SpotifyClone.Playlists.Application.Abstractions.Clients;
using SpotifyClone.Playlists.Application.Abstractions.Data;
using SpotifyClone.Playlists.Application.Abstractions.Repositories;
using SpotifyClone.Playlists.Application.Behaviors;
using SpotifyClone.Playlists.Application.Errors;
using SpotifyClone.Playlists.Application.Jobs;
using SpotifyClone.Playlists.Domain.Aggregates.Playlists;
using SpotifyClone.Playlists.Infrastructure.Clients;
using SpotifyClone.Playlists.Infrastructure.Persistence;
using SpotifyClone.Playlists.Infrastructure.Persistence.Database;
using SpotifyClone.Playlists.Infrastructure.Persistence.Queries;
using SpotifyClone.Playlists.Infrastructure.Persistence.Repositories;
using SpotifyClone.Shared.BuildingBlocks.Application.Abstractions;
using SpotifyClone.Shared.BuildingBlocks.Application.Abstractions.Mappers;

namespace SpotifyClone.Playlists.Infrastructure.DependencyInjection;

public static class PlaylistsModule
{
    public static IServiceCollection AddPlaylistsModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(
            PlaylistsApplicationAssemblyReference.Assembly));

        services.AddValidatorsFromAssembly(PlaylistsApplicationAssemblyReference.Assembly);

        services.AddDbContext<PlaylistsAppDbContext>(options => options.UseNpgsql(
            configuration.GetConnectionString("MainDb"),
            b => b.MigrationsAssembly(typeof(PlaylistsAppDbContext).Assembly.FullName)));

        RegisterClients(services, configuration);

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<IPlaylistsUnitOfWork>());
        services.AddScoped<IPlaylistsUnitOfWork, PlaylistsEfCoreUnitOfWork>();

        services.AddScoped<IPlaylistRepository, PlaylistEfCoreRepository>();
        services.AddScoped<ITrackReferenceRepository, TrackReferenceEfCoreRepository>();
        services.AddScoped<IUserReferenceRepository, UserReferenceEfCoreRepository>();
        services.AddScoped<IOutboxRepository, OutboxEfCoreRepository>();

        services.AddScoped<IPlaylistReadService, PlaylistEfCoreReadService>();

        services.AddScoped<IDomainExceptionMapper, PlaylistsDomainExceptionMapper>();

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(PlaylistsTransactionalPipelineBehavior<,>));
        services.AddTransient<ProcessOutboxMessagesJob>();

        return services;
    }

    public static async Task UsePlaylistsModule(this IApplicationBuilder app)
    {
        using IServiceScope scope = app.ApplicationServices.CreateScope();

        IRecurringJobManager recurringJobManager
            = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();
        recurringJobManager.AddOrUpdate<ProcessOutboxMessagesJob>(
            "playlists-outbox-processor",
            job => job.ProcessAsync(),
            "*/5 * * * * *" // Every 5 seconds (Cron expression)
        );
    }

    private static void RegisterClients(IServiceCollection services, IConfiguration configuration)
        => services.AddHttpClient<ICatalogModulePlaylistsClient, CatalogModulePlaylistsClient>(client =>
        {
            client.BaseAddress = new Uri(configuration["Application:ApiUrl"]!);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        });
}
