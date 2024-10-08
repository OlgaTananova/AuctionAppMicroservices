using AuctionService.Data;
using Microsoft.EntityFrameworkCore;
using MassTransit;
using Npgsql;
using AuctionService.Consumers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using AuctionService;
using Polly;

var builder = WebApplication.CreateBuilder(args);

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

builder.Services.AddControllers();
builder.Services.AddDbContext<AuctionDbContext>(opt =>
{
    opt.UseNpgsql(connectionString);
});


// Configure AutoMapper to map entities to DTOs

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

// Add and configure MassTransit for message-based communication
builder.Services.AddMassTransit(x =>
{
    // Configure outbox to handle messages when RabbitMQ is down
    x.AddEntityFrameworkOutbox<AuctionDbContext>(o =>
    {
        o.QueryDelay = TimeSpan.FromSeconds(10);
        o.UsePostgres();
        o.UseBusOutbox();
    });

    // Add consumers from the specified namespace
    x.AddConsumersFromNamespaceContaining<AuctionCreatedFaultConsumer>();

    // Set the endpoint name formatter to use kebab case
    x.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("auction", false));

    // Configure RabbitMQ with host and credentials from configuration
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.UseMessageRetry(r => {
            r.Handle<RabbitMqConnectionException>();
            r.Interval(5, TimeSpan.FromSeconds(10));
        });

        cfg.Host(builder.Configuration["RabbitMq:Host"], "/", host => {
            host.Username(builder.Configuration.GetValue("RabbitMq:Username", "guest"));
            host.Password(builder.Configuration.GetValue("RabbitMq:Password", "guest"));
        });
        cfg.ConfigureEndpoints(context);
    });
});

// Add JWT authentication to the service container
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = builder.Configuration["IdentityServiceUrl"];
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters.ValidateAudience = false;
        options.TokenValidationParameters.NameClaimType = "username";
    });

// Add scoped dependency injection for the AuctionRepository
builder.Services.AddScoped<IAuctionRepositiory, AuctionRepository>();  
builder.Services.AddGrpc();  

var app = builder.Build();

// Configure the HTTP request pipeline.

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapGrpcService<GrpcAuctionService>();

var retryPolicy = Policy
.Handle<NpgsqlException>()
.WaitAndRetry(5, retryAttempt => TimeSpan.FromSeconds(10));

retryPolicy.ExecuteAndCapture(() => DbInitializer.InitDb(app));

// Configure the Kestrel server to listen on port 7001
//app.Urls.Add("http://localhost:7001");

app.Run();

// Partial class definition for Program to support integration tests
public partial class Program { }
