using System.Text;
using System.Threading.RateLimiting;
using Hangfire;
using Hangfire.Redis.StackExchange;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using Serilog;
using SpotifyClone.Accounts.Application.Features.Accounts.Queries;
using SpotifyClone.Accounts.Application.Features.Accounts.Queries.List;
using SpotifyClone.Accounts.Infrastructure.DependencyInjection;
using SpotifyClone.Accounts.Infrastructure.Persistence.Accounts.Database;
using SpotifyClone.Accounts.Infrastructure.Persistence.Identity.Database;
using SpotifyClone.Billing.Infrastructure.DependencyInjection;
using SpotifyClone.Billing.Infrastructure.Persistence.Database;
using SpotifyClone.Catalog.Infrastructure.DependencyInjection;
using SpotifyClone.Catalog.Infrastructure.Persistence.Database;
using SpotifyClone.Catalog.Infrastructure.Persistence.Initialization;
using SpotifyClone.Playlists.Infrastructure.DependencyInjection;
using SpotifyClone.Playlists.Infrastructure.Persistence.Database;
using SpotifyClone.Search.Infrastructure.DependencyInjection;
using SpotifyClone.Shared.BuildingBlocks.Application.Abstractions.Primitives;
using SpotifyClone.Shared.BuildingBlocks.Application.Auth;
using SpotifyClone.Shared.BuildingBlocks.Application.Results;
using SpotifyClone.Shared.BuildingBlocks.Infrastructure.DependencyInjection;
using SpotifyClone.Streaming.Infrastructure.DependencyInjection;
using SpotifyClone.Streaming.Infrastructure.Notifications;
using SpotifyClone.Streaming.Infrastructure.Persistence.Database;
using Xabe.FFmpeg;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddBuildingBlocks(builder.Configuration);
builder.Services.AddAccountsModule(builder.Configuration);
builder.Services.AddCatalogModule(builder.Configuration);
builder.Services.AddStreamingModule(builder.Configuration);
builder.Services.AddPlaylistsModule(builder.Configuration);
builder.Services.AddBillingModule(builder.Configuration);
builder.Services.AddSearchModule(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

builder.Services.AddProblemDetails();

builder.Services.AddHttpContextAccessor();

builder.Services.AddHangfire(config => config
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UseRedisStorage(builder.Configuration.GetConnectionString("Redis")));
builder.Services.AddHangfireServer();

builder.Services.AddSignalR();

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],

            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"]!)),

            ClockSkew = TimeSpan.FromSeconds(30)
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                StringValues accessToken = context.Request.Query["access_token"];

                PathString path = context.HttpContext.Request.Path;

                // 3. ПЕРЕВІРКА: Ми допомагаємо ТІЛЬКИ SignalR запитам
                if (!string.IsNullOrEmpty(accessToken) &&
                    path.StartsWithSegments("/hubs/streaming"))
                {
                    context.Token = accessToken;
                }

                return Task.CompletedTask;
            }
        };
    })
    .AddGoogle(options =>
    {
        options.ClientId = builder.Configuration["Authentication:Google:ClientId"]!;
        options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"]!;
    });

builder.Services.AddAuthorization(options
    => options.AddPolicy(
        "EmailConfirmedPolicy",
        policy => policy.RequireClaim("email_confirmed", "true")));

builder.Services.AddHealthChecks();

builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("verification-limits", opt =>
    {
        opt.Window = TimeSpan.FromMinutes(15);
        opt.PermitLimit = 5;
        opt.QueueLimit = 0;
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
    });

    options.AddFixedWindowLimiter("send-limits", opt =>
    {
        opt.Window = TimeSpan.FromHours(1);
        opt.PermitLimit = 3;
        opt.QueueLimit = 0;
    });

    options.AddFixedWindowLimiter("login-limits", opt =>
    {
        opt.Window = TimeSpan.FromMinutes(15);
        opt.PermitLimit = 5;
        opt.QueueLimit = 0;
    });

    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

builder.Host.UseSerilog((ctx, lc) => lc
    .ReadFrom.Configuration(ctx.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console());

builder.Services.AddCors(options =>
options.AddPolicy(name: "DevCors",
        policy => policy.WithOrigins(
            "https://localhost:3000",
            "http://localhost:3000",
            "http://localhost:5000")
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials()));

if (Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true")
{
    FFmpeg.SetExecutablesPath("/usr/bin");
}
else
{
    string? ffmpegPath = builder.Configuration["FFmpegConfig:ExecutablesPath"];
    if (!string.IsNullOrEmpty(ffmpegPath))
    {
        FFmpeg.SetExecutablesPath(ffmpegPath);
    }
}

WebApplication app = builder.Build();

var provider = new FileExtensionContentTypeProvider();
provider.Mappings[".m3u8"] = "application/vnd.apple.mpegurl";
provider.Mappings[".mpd"] = "application/dash+xml";
provider.Mappings[".m4s"] = "video/iso.segment";

app.UseStaticFiles(new StaticFileOptions
{
    ContentTypeProvider = provider
});

app.MapStaticAssets();

app.UseRouting();

app.UseRateLimiter();

app.UseExceptionHandler();
app.UseStatusCodePages();

app.MapHub<StreamingHub>("/hubs/streaming");

app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health");

app.MapControllers();

app.UseCors("DevCors");

if (app.Environment.IsDevelopment())
{
    using (IServiceScope scope = app.Services.CreateScope())
    {
        IServiceProvider services = scope.ServiceProvider;

        AccountsAppDbContext accountsDb = services.GetRequiredService<AccountsAppDbContext>();
        await accountsDb.Database.MigrateAsync();

        IdentityAppDbContext identityDb = services.GetRequiredService<IdentityAppDbContext>();
        await identityDb.Database.MigrateAsync();

        StreamingAppDbContext streamingDb = services.GetRequiredService<StreamingAppDbContext>();
        await streamingDb.Database.MigrateAsync();

        CatalogAppDbContext catalogDb = services.GetRequiredService<CatalogAppDbContext>();
        await catalogDb.Database.MigrateAsync();

        PlaylistsAppDbContext playlistsDb = services.GetRequiredService<PlaylistsAppDbContext>();
        await playlistsDb.Database.MigrateAsync();

        BillingAppDbContext billingDb = services.GetRequiredService<BillingAppDbContext>();
        await billingDb.Database.MigrateAsync();
    }

    app.UseHttpsRedirection();
    app.UseDeveloperExceptionPage();

    app.MapHangfireDashboardWithNoAuthorizationFilters();

    app.MapOpenApi();
    app.MapScalarApiReference(options
        => options
            .WithTitle("SpotifyClone API")
            .WithTheme(ScalarTheme.Moon)
            .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient)
    );
}

await app.UseAccountsModule();
using (IServiceScope scope = app.Services.CreateScope())
{
    ISender sender = scope.ServiceProvider.GetRequiredService<ISender>();
    Result<UserList> adminResult = await sender.Send(new ListUsersQuery(new("Stopify"), new()));
    if (adminResult.IsFailure || adminResult.Value.Users.Items.Count > 1)
    {
        return;
    }

    ICurrentUser currentUser = scope.ServiceProvider.GetRequiredService<ICurrentUser>();
    currentUser.SetUser(adminResult.Value.Users.Items.Single().Id, UserRoles.Admin, true);

    await app.UseCatalogModule();
    await app.UseStreamingModule(CatalogSeeder.Tracks);
    await app.UsePlaylistsModule();
    await app.UseSearchModule();
}
app.UseBillingModule();

await app.RunAsync();
