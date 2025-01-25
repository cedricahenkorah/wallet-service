using DotNetEnv;
using WalletService.API.Configurations;
using WalletService.API.Repositories;
using WalletService.API.Services;

var builder = WebApplication.CreateBuilder(args);

Env.Load();

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Bind the "MongoDB" section of appsettings.json to MongoDbSettings
builder.Services.Configure<MongoDbSettings>(builder.Configuration.GetSection("MongoDB"));

// Replace placeholders with environment variables
var mongoConnectionString = Environment.GetEnvironmentVariable("MONGO_CONNECTION_STRING");
var mongoDatabaseName = Environment.GetEnvironmentVariable("MONGO_DATABASE_NAME");

builder.Configuration["MongoDB:ConnectionString"] =
    mongoConnectionString ?? throw new Exception("MONGO_CONNECTION_STRING not set.");
builder.Configuration["MongoDB:DatabaseName"] =
    mongoDatabaseName ?? throw new Exception("MONGO_DATABASE_NAME not set.");

// Register MongoDB context
builder.Services.AddSingleton<MongoDbContext>();

builder.Services.AddScoped<IWalletServices, WalletServices>();
builder.Services.AddScoped<IWalletRepository, WalletRepository>();

builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();

// Test MongoDB connection
using (var scope = app.Services.CreateScope())
{
    var mongoDbContext = scope.ServiceProvider.GetRequiredService<MongoDbContext>();
    try
    {
        if (mongoDbContext.TestConnection())
        {
            Console.WriteLine("MongoDB connection successful.");
        }
        else
        {
            Console.WriteLine("MongoDB connection failed.");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error testing MongoDB connection: {ex.Message}");
    }
}

app.Run();
