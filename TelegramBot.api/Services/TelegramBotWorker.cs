using TelegramBot.api.Data;

namespace TelegramBot.api.Services;

using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Microsoft.EntityFrameworkCore;




public class TelegramBotWorker : BackgroundService
{
    private readonly string _adminChatId;
    private readonly TelegramBotClient _botClient;
    private readonly IServiceScopeFactory _scopeFactory;

    public TelegramBotWorker(IConfiguration config, IServiceScopeFactory scopeFactory)
    {
        _adminChatId = config["Telegram:ChatId"] ?? throw new ArgumentNullException();
        _botClient = new TelegramBotClient(config["Telegram:Token"] ?? throw new ArgumentNullException());
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _botClient.StartReceiving(HandleUpdateAsync, HandleErrorAsync, new ReceiverOptions(), stoppingToken);
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
       
        if (update.Type == UpdateType.CallbackQuery && update.CallbackQuery != null)
        {
            var callback = update.CallbackQuery;
            var chatId = callback.Message!.Chat.Id.ToString();
            var data = callback.Data;

            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            
            if (data == "show_menu")
            {
                await ShowMainMenu(botClient, chatId, cancellationToken);
                return;
            }
            if (data!.StartsWith("category_"))
            {
                int categoryId = int.Parse(data.Split('_')[1]);
                await ShowProducts(botClient, chatId, categoryId, db, cancellationToken);
                return;
            }

            if (data.StartsWith("buy_"))
            {
                int productId = int.Parse(data.Split('_')[1]);
                var product = await db.Products.Include(p => p.Category).FirstOrDefaultAsync(p => p.Id == productId);

                if (product != null)
                {
                    await ProcessOrder(botClient, callback, product, cancellationToken);
                }
                return;
            }
        }


        if (update.Type == UpdateType.Message && update.Message?.Text != null)
        {
            await ShowMainMenu(botClient, update.Message.Chat.Id.ToString(), cancellationToken);
        }
    }
    
    private async Task ShowMainMenu(ITelegramBotClient botClient, string chatId, CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var categories = await db.Categories.ToListAsync(ct);

        var buttons = categories.Select(c => 
            new[] { InlineKeyboardButton.WithCallbackData($"{c.Name}", $"category_{c.Id}") }
        ).ToArray();

        await botClient.SendMessage(chatId, "📂 *Выберите категорию:*", 
            parseMode: ParseMode.Markdown, replyMarkup: new InlineKeyboardMarkup(buttons), cancellationToken: ct);
    }
    private async Task ShowProducts(ITelegramBotClient botClient, string chatId, int catId, AppDbContext db, CancellationToken ct)
    {
        var products = await db.Products.Where(p => p.CategoryId == catId).ToListAsync(ct);

        var buttons = products.Select(p => 
            new[] { InlineKeyboardButton.WithCallbackData($"{p.Name} — {p.Price} ₸", $"buy_{p.Id}") }
        ).ToList();
        
        buttons.Add(new[] { InlineKeyboardButton.WithCallbackData("🔙 Назад", "show_menu") });

        await botClient.SendMessage(chatId, "🛒 *Выберите товар:*", 
            parseMode: ParseMode.Markdown, replyMarkup: new InlineKeyboardMarkup(buttons), cancellationToken: ct);
    }
    private async Task ProcessOrder(ITelegramBotClient botClient, CallbackQuery callback, Models.Product product, CancellationToken ct)
    {
        var chatId = callback.Message!.Chat.Id.ToString();
        var username = callback.From.Username ?? callback.From.FirstName;
        
        await botClient.SendMessage(chatId, $"✅ Заказ принят: *{product.Name}*\nСумма: *{product.Price} ₸*", 
            parseMode: ParseMode.Markdown, 
            replyMarkup: new InlineKeyboardMarkup(InlineKeyboardButton.WithCallbackData("🔙 В меню", "show_menu")),
            cancellationToken: ct);
        
        await botClient.SendMessage(_adminChatId, $"🔥 *НОВЫЙ ЗАКАЗ!*\nТовар: {product.Name}\nКатегория: {product.Category?.Name}\nКлиент: @{username}", 
            parseMode: ParseMode.Markdown, cancellationToken: ct);
    }

    private Task HandleErrorAsync(ITelegramBotClient botClient, Exception ex, HandleErrorSource s, CancellationToken ct) 
    { 
        Console.WriteLine(ex.Message); return Task.CompletedTask; 
    }
}