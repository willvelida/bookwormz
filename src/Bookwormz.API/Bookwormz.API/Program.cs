using Bookwormz.API.Repository;
using Bookwormz.API.Repository.Interfaces;
using Bookwormz.API.Services;
using Bookwormz.API.Services.Interfaces;
using Microsoft.Azure.Cosmos;

var builder = WebApplication.CreateBuilder(args);

var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables()
    .Build();

// Add services to the container.
builder.Services.AddSingleton<IConfiguration>(configuration);
builder.Services.AddLogging();

builder.Services.AddSingleton(sp =>
{
    IConfiguration configuration = sp.GetService<IConfiguration>();
    CosmosClientOptions cosmosClientOptions = new CosmosClientOptions
    {
        MaxRetryAttemptsOnRateLimitedRequests = 5,
        MaxRetryWaitTimeOnRateLimitedRequests = TimeSpan.FromSeconds(5),
    };
    // TODO: Change to use Managed Identity
    return new CosmosClient(configuration["testcosmosstring"], cosmosClientOptions);
});

builder.Services.AddTransient<IBookRepository, BookRepository>();
builder.Services.AddTransient<IBookService, BookService>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
