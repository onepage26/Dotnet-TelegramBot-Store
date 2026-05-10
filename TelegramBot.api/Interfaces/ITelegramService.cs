namespace TelegramBot.api.Interfaces;


public interface ITelegramService
{ 
    Task<bool> SendOrderNotificationAsync(string orderCode, double price);
}