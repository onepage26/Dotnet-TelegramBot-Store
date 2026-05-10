
using Microsoft.EntityFrameworkCore;
using TelegramBot.api.Data;
using TelegramBot.api.Interfaces;
using TelegramBot.api.Services;

var builder = WebApplication.CreateBuilder(args);

// 1. Добавляем поддержку контроллеров
builder.Services.AddControllers();


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddHostedService<TelegramBotWorker>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// 3. Мапим контроллеры
app.MapControllers();

app.Run();