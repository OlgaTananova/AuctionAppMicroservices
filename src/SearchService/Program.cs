using MongoDB.Driver;
using MongoDB.Entities;
using Polly;
using Polly.Extensions.Http;
using SearchService;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddHttpClient<AuctionServiceHttpClient>().AddPolicyHandler(GetPolicy());

var app = builder.Build();

// Configure the HTTP request pipeline.


app.UseAuthorization();

app.MapControllers();

// wrap a call to the Auction Service in the DbIntializer method in a block that runs after the application has started. 
app.Lifetime.ApplicationStarted.Register(async () =>{
  try
{
    await DbInitializer.InitDb(app);
}
catch (Exception e)
{
    Console.WriteLine(e.ToString());
}  
});

app.Run();

// static method from Polly library that allows to retry to connect to the AuctionService if http requests fail
static IAsyncPolicy<HttpResponseMessage> GetPolicy()
=> HttpPolicyExtensions
    .HandleTransientHttpError()
    .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.NotFound)
    .WaitAndRetryForeverAsync(_ => TimeSpan.FromSeconds(5));