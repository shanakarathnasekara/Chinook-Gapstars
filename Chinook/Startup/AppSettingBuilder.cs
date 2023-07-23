namespace Chinook.Startup
{
    public static class AppSettingBuilder
    {
        public static IHostBuilder ConfigureAppSettings(this IHostBuilder host)
        {
            var enviroment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            host.ConfigureAppConfiguration((ctx, builder) =>
            {
                builder.AddJsonFile("appsettings.json", false, true);
                builder.AddJsonFile($"appsettings.{enviroment}.json", true, true);
                builder.AddEnvironmentVariables();
            });

            return host;
        }
    }
}
