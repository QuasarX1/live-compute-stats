using display_stats.Data;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace display_stats
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            bool development = args.Contains("--dev");

            // Add configuration files
            string environment_specific_settings_file = "production.appsettings.json";
            if (development)
            {
                environment_specific_settings_file = "development.appsettings.json";
            }
            string environment_specific_settings_filepath = Path.Combine(Directory.GetCurrentDirectory(), environment_specific_settings_file);

            if (File.Exists(environment_specific_settings_filepath))
            {
                builder.Configuration.AddJsonFile(environment_specific_settings_file, false, reloadOnChange: false);
            }
            else
            {
                File.Copy(Path.Combine(Directory.GetCurrentDirectory(), "EXAMPLE_production.appsettings.json"), environment_specific_settings_filepath);
                throw new FileNotFoundException(String.Format("Settings file for {0} environment not found. New file created - please edit the file and re-launch.", (development) ? "development" : "production"));
            }

            // Add services to the container.
            builder.Services.AddRazorPages();
            builder.Services.AddServerSideBlazor();
            builder.Services.AddSingleton<WeatherForecastService>();// Lasily instantiated and shared by all requests
            //builder.Services.AddSingleton<TestDataService>();
            builder.Services.AddSingleton<ServerStatusService>(new ServerStatusService(builder.Configuration));// Eagerly instantiated and shared by all requests

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseStaticFiles();

            app.UseRouting();

            app.MapBlazorHub();
            app.MapFallbackToPage("/_Host");

            app.Run();
        }
    }
}