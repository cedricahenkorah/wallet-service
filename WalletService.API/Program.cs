using DotNetEnv;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
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

var jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET");
var jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER");
var jwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE");

builder.Configuration["MongoDB:ConnectionString"] =
    mongoConnectionString ?? throw new Exception("MONGO_CONNECTION_STRING not set.");
builder.Configuration["MongoDB:DatabaseName"] =
    mongoDatabaseName ?? throw new Exception("MONGO_DATABASE_NAME not set.");
builder.Configuration["Jwt:Secret"] = jwtSecret ?? throw new Exception("JWT_SECRET not set.");
builder.Configuration["Jwt:Issuer"] = jwtIssuer ?? throw new Exception("JWT_ISSUER not set.");
builder.Configuration["Jwt:Audience"] = jwtAudience ?? throw new Exception("JWT_AUDIENCE not set.");

// Register MongoDB context
builder.Services.AddSingleton<MongoDbContext>();

// Register JWT authentication
builder
    .Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                System.Text.Encoding.UTF8.GetBytes(jwtSecret)
            ),
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,
            ValidateAudience = true,
            ValidAudience = jwtAudience,
            ValidateLifetime = true,
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddScoped<IWalletServices, WalletServices>();
builder.Services.AddScoped<IWalletRepository, WalletRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

builder.Services.AddControllers();

// Configure built-in logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Logging.AddEventSourceLogger();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

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
