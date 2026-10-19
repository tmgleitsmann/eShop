using System.Text.Json;
using System.Text.Json.Serialization;
using DiscountService;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOptions<DiscountOptions>()
    .Bind(builder.Configuration.GetSection(DiscountOptions.SectionName));
builder.Services.AddSingleton<DiscountCalculator>();
builder.Services.ConfigureHttpJsonOptions(options =>
{
    // Newtonsoft accepted quoted numbers; keep that compatibility without using doubles.
    options.SerializerOptions.NumberHandling = JsonNumberHandling.AllowReadingFromString;
});

var app = builder.Build();

app.MapGet("/api/discount/{id?}", async (
    string? customerId,
    decimal amount,
    int loyaltyYears,
    DiscountCalculator calculator,
    ILogger<DiscountCalculator> logger,
    CancellationToken cancellationToken) =>
{
    if (string.IsNullOrWhiteSpace(customerId))
    {
        return Results.BadRequest();
    }

    var result = calculator.Calculate(customerId, amount, loyaltyYears);
    // Preserve the legacy GET-only simulated mainframe wait without blocking a thread.
    await Task.Delay(150, cancellationToken);
    WriteAudit(logger, result);
    return Results.Ok(result);
});

app.MapPost("/api/discount/{id?}", async (
    HttpRequest httpRequest,
    DiscountCalculator calculator,
    ILogger<DiscountCalculator> logger,
    CancellationToken cancellationToken) =>
{
    if (!httpRequest.HasJsonContentType())
    {
        return Results.StatusCode(StatusCodes.Status415UnsupportedMediaType);
    }

    DiscountRequest? request;
    try
    {
        request = await httpRequest.ReadFromJsonAsync<DiscountRequest>(cancellationToken);
    }
    catch (JsonException)
    {
        return Results.BadRequest();
    }

    if (request is null)
    {
        return Results.BadRequest();
    }

    var result = calculator.Calculate(request.CustomerId, request.Amount, request.LoyaltyYears);
    WriteAudit(logger, result);
    return Results.Ok(result);
});

app.Run();

static void WriteAudit(ILogger logger, DiscountResult result) =>
    logger.LogInformation("{AuditTimestamp} {AuditRecord}",
        DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), result.ToString());

public partial class Program;
