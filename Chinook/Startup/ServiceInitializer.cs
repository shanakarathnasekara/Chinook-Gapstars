using Microsoft.EntityFrameworkCore;
using Chinook.Areas.Identity;
using Microsoft.AspNetCore.Components.Authorization;
using Chinook.Models;
using Chinook.Services.Playlist;
using Chinook.Services.Albums;
using Chinook.Services.Artists;
using Chinook.Services.EventsStreaming;
using Chinook.Services.Tracks;
using Chinook.Services.Users;

namespace Chinook.Startup
{
    public static partial class ServiceInitializer
    {
        public static IServiceCollection ConfigureServices(this IServiceCollection services, IConfiguration config, IWebHostEnvironment env)
        {
            RegisterServices(services);
            RegisterDatabaseInitializer(services, config);
            RegisterOtherInitializers(services);
            return services;
        }

        public static void RegisterServices(IServiceCollection services)
        {
            services.AddScoped<IPlaylistService, PlaylistService>();
            services.AddScoped<IAlbumService, AlbumService>();
            services.AddScoped<IArtistService, ArtistService>();
            services.AddScoped<ITrackService, TrackService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<EventsService>();
        }

        public static void RegisterDatabaseInitializer(IServiceCollection services, IConfiguration config)
        {
            var connectionString = config.GetConnectionString("DefaultConnection");
            services.AddDbContextFactory<ChinookContext>(opt => opt.UseSqlite(connectionString));
            services.AddDatabaseDeveloperPageExceptionFilter();
            services.AddDbContext<ChinookContext>();

            services.AddDefaultIdentity<ChinookUser>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddEntityFrameworkStores<ChinookContext>();

        }

        public static void RegisterOtherInitializers(IServiceCollection services)
        {
            services.AddRazorPages();
            services.AddServerSideBlazor();
            services.AddScoped<AuthenticationStateProvider, RevalidatingIdentityAuthenticationStateProvider<ChinookUser>>();
        }
    }
}
