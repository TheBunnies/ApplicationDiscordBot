using ApplicationDiscordBot.Contracts;
using ApplicationDiscordBot.Models;
using ApplicationDiscordBot.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.Configure<BotConfiguration>(builder.Configuration.GetSection("AppSettings"));
builder.Services.AddHttpClient<BotService>();
builder.Services.AddSingleton<IBotService, BotService>();
builder.Services.AddHostedService<BotHostedService>();


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