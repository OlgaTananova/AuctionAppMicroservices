using Duende.IdentityServer;
using IdentityService.Data;
using IdentityService.Models;
using IdentityService.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace IdentityService;

internal static class HostingExtensions
{
    public static WebApplication ConfigureServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddRazorPages();
       

        // Add User Secrets Manager to the container
        builder.Configuration.AddUserSecrets<Program>();

        // Get the secrets from user secrets
        var configuration = builder.Configuration;
        var username = configuration["PostgresUser"];
        var password = configuration["PostgresPassword"];
        var database = configuration["Database"];
      

        // Construct the connection string
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
            .Replace("{PostgresUser}", username)
            .Replace("{PostgresPassword}", password)
            .Replace("{Database}", database);
       

        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString));

        builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        builder.Services
            .AddIdentityServer(options =>
            {
                options.Events.RaiseErrorEvents = true;
                options.Events.RaiseInformationEvents = true;
                options.Events.RaiseFailureEvents = true;
                options.Events.RaiseSuccessEvents = true;

                // see https://docs.duendesoftware.com/identityserver/v6/fundamentals/resources/
                //options.EmitStaticAudienceClaim = true;
            })
            .AddInMemoryIdentityResources(Config.IdentityResources)
            .AddInMemoryApiScopes(Config.ApiScopes)
            .AddInMemoryClients(Config.Clients)
            .AddAspNetIdentity<ApplicationUser>()
            .AddProfileService<CustomProfileService>();

        // Adjust app cookies
        builder.Services.ConfigureApplicationCookie(options =>
        {
            options.Cookie.SameSite = SameSiteMode.Lax;
        });

        builder.Services.AddAuthentication()
            .AddGoogle(options =>
            {
                options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
                // register your Id Server with Google at https://console.developers.google.com
                // enable the Google API
                // set the redirect URI to https://localhost:5000/signin-google

                options.ClientId = "copy client ID from Google here";
                options.ClientSecret = "copy client secret from Google here";
            });


        return builder.Build();
    }
    
    public static WebApplication ConfigurePipeline(this WebApplication app)
    { 
        app.UseSerilogRequestLogging();
    
        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseStaticFiles();
        app.UseRouting();
        app.UseIdentityServer();
        app.UseAuthorization();
        
        app.MapRazorPages()
            .RequireAuthorization();

        return app;
    }
}