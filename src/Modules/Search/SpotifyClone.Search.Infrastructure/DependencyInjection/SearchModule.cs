using FluentValidation;
using Hangfire;
using Meilisearch;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SpotifyClone.Search.Application;
using SpotifyClone.Search.Application.Abstractions.Clients;
using SpotifyClone.Search.Application.Abstractions.Services;
using SpotifyClone.Search.Application.Jobs;
using SpotifyClone.Search.Infrastructure.Clients;
using SpotifyClone.Search.Infrastructure.Services;

namespace SpotifyClone.Search.Infrastructure.DependencyInjection;

public static class SearchModule
{
    public static IServiceCollection AddSearchModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(
            SearchApplicationAssemblyReference.Assembly));

        services.AddValidatorsFromAssembly(SearchApplicationAssemblyReference.Assembly);

        services.AddHostedService<MeiliSearchIndexInitializer>();
        services.Configure<MeiliSearchOptions>(configuration.GetSection("MeiliSearch"));
        RegisterClients(services, configuration);

        services.AddSingleton(sp =>
        {
            MeiliSearchOptions options = sp.GetRequiredService<IOptions<MeiliSearchOptions>>().Value;
            return new MeilisearchClient(options.Endpoint, options.MasterKey);
        });
        services.AddSingleton<ISearchIndexer, MeiliSearchService>();
        services.AddSingleton<ISearchProvider, MeiliSearchService>();

        services.AddTransient<FullReindexJob>();

        return services;
    }

    public static async Task UseSearchModule(this IApplicationBuilder app)
    {
        using IServiceScope scope = app.ApplicationServices.CreateScope();

        IRecurringJobManager recurringJobManager
            = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();
        recurringJobManager.AddOrUpdate<FullReindexJob>(
            "search-full-reindex",
            job => job.ProcessAsync(),
            Cron.Daily(3)
        );
    }

    private static void RegisterClients(IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpClient<IAccountsModuleSearchClient, AccountsModuleSearchClient>(client =>
        {
            client.BaseAddress = new Uri(configuration["Application:ApiUrl"]!);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        });

        services.AddHttpClient<ICatalogModuleSearchClient, CatalogModuleSearchClient>(client =>
        {
            client.BaseAddress = new Uri(configuration["Application:ApiUrl"]!);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        });

        services.AddHttpClient<IPlaylistsModuleSearchClient, PlaylistsModuleSearchClient>(client =>
        {
            client.BaseAddress = new Uri(configuration["Application:ApiUrl"]!);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        });
    }
}
