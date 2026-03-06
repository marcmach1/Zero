using Telegram.Bot;

namespace Zero.Services
{
    public class NotificationService
    {
        private readonly TelegramBotClient _botClient;
        private readonly long _chatId; // ChatId agora é long na biblioteca nova

        public NotificationService(IConfiguration config)
        {
            var token = config["TelegramToken"];
            var chatIdStr = config["TelegramChatId"];

            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(chatIdStr))
            {
                throw new Exception("Configurações do Telegram ausentes no appsettings.json");
            }

            _botClient = new TelegramBotClient(token);
            _chatId = long.Parse(chatIdStr);
        }

        public async Task EnviarMensagem(string mensagem)
        {
            // Na versão 22.x, o método é esse aqui:
            await _botClient.SendMessage(_chatId, mensagem);
        }
    }
}