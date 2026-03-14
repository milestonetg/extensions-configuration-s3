using MilestoneTG.Extensions.Configuration.S3.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonS3Object("mtg-test-config", "mySettings.json", reloadAfter: TimeSpan.FromSeconds(10));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.MapControllers();

app.Run();
