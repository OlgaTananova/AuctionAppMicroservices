using Microsoft.AspNetCore.Authentication.JwtBearer;

var builder = WebApplication.CreateBuilder(args);

// Configure reverse proxy using settings from configuration

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// Add JWT authentication to the service container
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // Set the authority to validate the token issuer
        options.Authority = builder.Configuration["IdentityServiceUrl"];
        // Disable HTTPS metadata requirement for development purposes
        options.RequireHttpsMetadata = false;
        // Disable audience validation
        options.TokenValidationParameters.ValidateAudience = false;
        // Set the claim type for the username
        options.TokenValidationParameters.NameClaimType = "username";
    });
var app = builder.Build();

app.MapReverseProxy();
app.UseAuthentication();
app.UseAuthorization();

app.Run();
