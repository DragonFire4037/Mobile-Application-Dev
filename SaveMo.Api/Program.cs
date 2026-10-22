
var builder = WebApplication.CreateBuilder(args);

// Register MVC controllers.
builder.Services.AddControllers();

// Enable OpenAPI document generation.
builder.Services.AddOpenApi();

// Standardize unexpected error responses.
builder.Services.AddProblemDetails();

var app = builder.Build();

// Handle unexpected exceptions centrally.
app.UseExceptionHandler();

// Redirect HTTP requests to HTTPS.
app.UseHttpsRedirection();

// Expose the OpenAPI document during development.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Map controller routes.
app.MapControllers();

app.Run();
