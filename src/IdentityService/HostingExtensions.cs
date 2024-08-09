using Duende.IdentityServer;
using IdentityService.Data;
using IdentityService.Models;
using IdentityService.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace IdentityService;

// Static class containing extension methods for configuring the web host
internal static class HostingExtensions
{
    // Extension method to configure services
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

        // Add the DbContext for the application using PostgreSQL

        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString));

        // Add ASP.NET Core Identity services and configure them to use Entity Framework

        builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        // Add and configure IdentityServer services
        builder.Services
            .AddIdentityServer(options =>
            {
                // Enable various event types for logging and monitoring
                options.Events.RaiseErrorEvents = true;
                options.Events.RaiseInformationEvents = true;
                options.Events.RaiseFailureEvents = true;
                options.Events.RaiseSuccessEvents = true;

                // Set the issuer URI for the IdentityServer when running in a Docker environment
                if (builder.Environment.IsEnvironment("Docker")){
                    options.IssuerUri ="identity-svc";
                }

                // see https://docs.duendesoftware.com/identityserver/v6/fundamentals/resources/
                // Uncomment the following line if you need to emit the static audience claim
                //options.EmitStaticAudienceClaim = true;
            })
            .AddInMemoryIdentityResources(Config.IdentityResources) 
            .AddInMemoryApiScopes(Config.ApiScopes) 
            .AddInMemoryClients(Config.Clients(builder.Configuration))
            .AddAspNetIdentity<ApplicationUser>()
            .AddProfileService<CustomProfileService>();

        // Configure application cookies to use lax SameSite mode

        builder.Services.ConfigureApplicationCookie(options =>
        {
            options.Cookie.SameSite = SameSiteMode.Lax;
        });

        // Add Google Athentication support
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

    // Extension method to configure the HTTP request pipeline
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