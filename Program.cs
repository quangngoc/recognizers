using Microsoft.OpenApi.Models;
using QuangNgoc.Recognizers.Contracts;
using QuangNgoc.Recognizers.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<ITextRecognizerService, TextRecognizerService>();

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen(options =>
{
    options.EnableAnnotations();
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Recognizers",
        Version = "v1"
    });
});

var app = builder.Build();

app.MapOpenApi();
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.DocumentTitle = "Recognizers";
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Recognizers v1");
});

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();