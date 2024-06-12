using AuctionService.Data;
using Microsoft.EntityFrameworkCore;
using Npgsql;

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


// Automappe to map entities to DTOs
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

var app = builder.Build();

// Configure the HTTP request pipeline.

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseAuthorization();

app.MapControllers();

try
{

    DbInitializer.InitDb(app);

}
catch (Exception e)
{

    Console.WriteLine(e.Message);
}
// Configure the Kestrel server to listen on port 7001
//app.Urls.Add("http://localhost:7001");

app.Run();
