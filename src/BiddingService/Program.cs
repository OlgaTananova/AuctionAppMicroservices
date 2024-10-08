using BiddingService;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using MongoDB.Driver;
using MongoDB.Entities;
using Polly;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();


// Add and configure MassTransit for message-based communication
builder.Services.AddMassTransit(x =>
{
    // Add consumers
    x.AddConsumersFromNamespaceContaining<AuctionCreatedConsumer>();


    // Set the endpoint name formatter to use kebab case
    x.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("bids", false));

    // Configure RabbitMQ with host and credentials from configuration
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.UseMessageRetry(r =>
        {
            r.Handle<RabbitMqConnectionException>();
            r.Interval(5, TimeSpan.FromSeconds(10));
        });
        cfg.Host(builder.Configuration["RabbitMq:Host"], "/", host =>
        {
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

// Add Automapper to map entities to dtos
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
// Add background service to check the finished auctions
builder.Services.AddHostedService<CheckAuctionFinished>();
// Add Grpc service to the container
builder.Services.AddScoped<GrpcAuctionClient>();

var app = builder.Build();

app.UseAuthorization();

app.MapControllers();

await Policy.Handle<TimeoutException>()
  .WaitAndRetryAsync(5, retryAttempt => TimeSpan.FromSeconds(10))
  .ExecuteAndCaptureAsync(async () =>
  {
      await DB.InitAsync("BidDb", MongoClientSettings
            .FromConnectionString(builder.Configuration.GetConnectionString("BidDbConnection")));
  });


app.Run();
