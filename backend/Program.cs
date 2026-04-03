using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<TodoDb>(opt =>
    opt.UseSqlite("Data Source=todos.db"));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Como Weather API",
        Version = "v1",
        Description = "7-day weather forecast API for Como, Italy"
    });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddHttpClient<IOpenMeteoClient, OpenMeteoClient>();
builder.Services.AddScoped<IWeatherService, WeatherService>();
builder.Services.AddMemoryCache();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<TodoDb>();
    db.Database.EnsureCreated();
}

app.MapGet("/api/health", () =>
{
    return Results.Ok(new
    {
        status = "healthy",
        timestamp = DateTime.UtcNow.ToString("o")
    });
})
.WithName("HealthCheck")
.WithOpenApi()
.Produces(StatusCodes.Status200OK);

app.MapGet("/api/weather/como", async (IWeatherService weatherService) =>
{
    try
    {
        var forecast = await weatherService.GetComoWeatherForecastAsync();
        return Results.Ok(forecast);
    }
    catch (HttpRequestException ex)
    {
        return Results.StatusCode(StatusCodes.Status503ServiceUnavailable);
    }
    catch (Exception ex)
    {
        return Results.StatusCode(StatusCodes.Status500InternalServerError);
    }
})
.WithName("GetComoWeatherForecast")
.WithOpenApi()
.Produces(StatusCodes.Status200OK)
.Produces(StatusCodes.Status503ServiceUnavailable)
.Produces(StatusCodes.Status500InternalServerError);

app.MapGet("/todos", async (TodoDb db) =>
    await db.Todos.ToListAsync())
.WithName("GetAllTodos")
.WithOpenApi()
.Produces<List<Todo>>(StatusCodes.Status200OK);

app.MapGet("/todos/{id}", async (int id, TodoDb db) =>
    await db.Todos.FindAsync(id)
        is Todo todo
            ? Results.Ok(todo)
            : Results.NotFound())
.WithName("GetTodoById")
.WithOpenApi()
.Produces<Todo>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status404NotFound);

app.MapPost("/todos", async (CreateTodoRequest request, TodoDb db) =>
{
    if (string.IsNullOrWhiteSpace(request.Title))
    {
        return Results.BadRequest("Title is required");
    }

    var todo = new Todo
    {
        Title = request.Title,
        IsCompleted = false
    };

    db.Todos.Add(todo);
    await db.SaveChangesAsync();

    return Results.Created($"/todos/{todo.Id}", todo);
})
.WithName("CreateTodo")
.WithOpenApi()
.Produces<Todo>(StatusCodes.Status201Created)
.Produces(StatusCodes.Status400BadRequest);

app.MapPut("/todos/{id}", async (int id, UpdateTodoRequest request, TodoDb db) =>
{
    var todo = await db.Todos.FindAsync(id);

    if (todo is null)
        return Results.NotFound();

    if (!string.IsNullOrWhiteSpace(request.Title))
        todo.Title = request.Title;

    if (request.IsCompleted.HasValue)
        todo.IsCompleted = request.IsCompleted.Value;

    await db.SaveChangesAsync();

    return Results.Ok(todo);
})
.WithName("UpdateTodo")
.WithOpenApi()
.Produces<Todo>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status404NotFound);

app.MapDelete("/todos/{id}", async (int id, TodoDb db) =>
{
    if (await db.Todos.FindAsync(id) is Todo todo)
    {
        db.Todos.Remove(todo);
        await db.SaveChangesAsync();
        return Results.NoContent();
    }

    return Results.NotFound();
})
.WithName("DeleteTodo")
.WithOpenApi()
.Produces(StatusCodes.Status204NoContent)
.Produces(StatusCodes.Status404NotFound);

app.Run();

public class Todo
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
}

public class CreateTodoRequest
{
    public string Title { get; set; } = string.Empty;
}

public class UpdateTodoRequest
{
    public string? Title { get; set; }
    public bool? IsCompleted { get; set; }
}

public class TodoDb : DbContext
{
    public TodoDb(DbContextOptions<TodoDb> options) : base(options) { }

    public DbSet<Todo> Todos => Set<Todo>();
}

public interface IOpenMeteoClient
{
    Task<OpenMeteoResponse> GetForecastAsync(double latitude, double longitude);
}

public class OpenMeteoClient : IOpenMeteoClient
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public OpenMeteoClient(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public async Task<OpenMeteoResponse> GetForecastAsync(double latitude, double longitude)
    {
        var baseUrl = _configuration["OPEN_METEO_BASE_URL"] ?? "https://api.open-meteo.com/v1";
        var url = $"{baseUrl}/forecast?latitude={latitude}&longitude={longitude}&daily=temperature_2m_max,temperature_2m_min,precipitation_sum,windspeed_10m_max,weather_code&timezone=auto";

        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var options = new System.Text.Json.JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.SnakeCaseLower
        };
        return System.Text.Json.JsonSerializer.Deserialize<OpenMeteoResponse>(content, options)
            ?? throw new InvalidOperationException("Failed to deserialize weather data");
    }
}

public interface IWeatherService
{
    Task<WeatherForecastResponse> GetComoWeatherForecastAsync();
}

public class WeatherService : IWeatherService
{
    private readonly IOpenMeteoClient _openMeteoClient;
    private readonly IMemoryCache _cache;
    private readonly IConfiguration _configuration;
    private const string CacheKey = "como_weather_forecast";
    private const int CacheDurationMinutes = 60;

    public WeatherService(IOpenMeteoClient openMeteoClient, IMemoryCache cache, IConfiguration configuration)
    {
        _openMeteoClient = openMeteoClient;
        _cache = cache;
        _configuration = configuration;
    }

    public async Task<WeatherForecastResponse> GetComoWeatherForecastAsync()
    {
        if (_cache.TryGetValue(CacheKey, out WeatherForecastResponse? cachedForecast))
        {
            return cachedForecast!;
        }

        var latitude = double.Parse(_configuration["COMO_LATITUDE"] ?? "45.8081");
        var longitude = double.Parse(_configuration["COMO_LONGITUDE"] ?? "9.0852");

        var openMeteoResponse = await _openMeteoClient.GetForecastAsync(latitude, longitude);

        var forecast = new WeatherForecastResponse
        {
            Latitude = openMeteoResponse.Latitude,
            Longitude = openMeteoResponse.Longitude,
            Timezone = openMeteoResponse.Timezone,
            Daily = openMeteoResponse.Daily
        };

        var cacheOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromMinutes(CacheDurationMinutes));

        _cache.Set(CacheKey, forecast, cacheOptions);

        return forecast;
    }
}

public class OpenMeteoResponse
{
    [JsonPropertyName("latitude")]
    public double Latitude { get; set; }

    [JsonPropertyName("longitude")]
    public double Longitude { get; set; }

    [JsonPropertyName("timezone")]
    public string Timezone { get; set; } = string.Empty;

    [JsonPropertyName("daily")]
    public DailyData Daily { get; set; } = new();
}

public class DailyData
{
    [JsonPropertyName("time")]
    public List<string> Time { get; set; } = new();

    [JsonPropertyName("temperature_2m_max")]
    public List<double> Temperature2mMax { get; set; } = new();

    [JsonPropertyName("temperature_2m_min")]
    public List<double> Temperature2mMin { get; set; } = new();

    [JsonPropertyName("precipitation_sum")]
    public List<double> PrecipitationSum { get; set; } = new();

    [JsonPropertyName("windspeed_10m_max")]
    public List<double> Windspeed10mMax { get; set; } = new();

    [JsonPropertyName("weather_code")]
    public List<int> WeatherCode { get; set; } = new();
}

public class WeatherForecastResponse
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string Timezone { get; set; } = string.Empty;
    public DailyData Daily { get; set; } = new();
}