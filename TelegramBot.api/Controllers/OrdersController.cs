using TelegramBot.api.Interfaces;



using Microsoft.AspNetCore.Mvc;

namespace KaspiOrderSync.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController(ITelegramService telegramService) : ControllerBase
{
    [HttpPost("create")]
    public async Task<IActionResult> CreateOrder()
    {
        var randomCode = "ORD-" + new Random().Next(1000, 9999);
        var randomPrice = new Random().Next(5000, 150000);

        var isSent = await telegramService.SendOrderNotificationAsync(randomCode, randomPrice);

        if (isSent)
        {
            return Ok($"Успех! Заказ {randomCode} создан, уведомление отправлено в Telegram.");
        }

        return BadRequest("Ошибка при отправке уведомления.");
    }
}