using Telegram.Bot;
using Telegram.Bot.Types;   
using Telegram.Bot.Types.ReplyMarkups;


internal class SessionState
{
    public string Key { get; set; }
    public bool Send { get; set; }
    public bool Asked { get; set; }
}

internal class Program
{
    private static Dictionary<long, SessionState> userSessions = [];

    private static void Main(string[] args)
    {   
        

        var botClient = new TelegramBotClient("{your token}");

        botClient.StartReceiving(Update, Error);
        Console.ReadLine();


        async Task Update(ITelegramBotClient client, Update update, CancellationToken token)
        {
            if (update.CallbackQuery != null)
            {
                string callbackData = update.CallbackQuery.Data;
                long chatId = update.CallbackQuery.Message.Chat.Id;

                if (callbackData == "Написать")
                {
                    await botClient.SendTextMessageAsync(chatId, "Можешь задавать свой вопрос!");
                    userSessions[update.CallbackQuery.Message.Chat.Id].Send = true;
                    await botClient.AnswerCallbackQueryAsync(update.CallbackQuery.Id);
                }
            }
            else if (update.Message != null)
            {
                var message = update.Message;
                long chatId = message.Chat.Id;

                if (message.Text == null)
                {
                    await botClient.SendTextMessageAsync(chatId, "Отправь текстом!", cancellationToken: token);
                    return;
                }

                InlineKeyboardMarkup inlineKeyboard = new(new[]
                {
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData(text: "задать вопрос", callbackData: "Написать"),
                    },
                });

                Console.WriteLine($"{message.Text}   |   @{message.Chat.Username}");

                if (!userSessions.ContainsKey(chatId))
                {
                    userSessions[chatId] = new SessionState();
                }

                var session = userSessions[chatId];


                if (message.Text.Contains("/start") && message.Text.Length > 10)
                {
                    session.Key = message.Text.Split(" ")[1];
                    session.Asked = false;
                }

                if (!session.Asked)
                {
                    await botClient.SendTextMessageAsync(chatId, "Привет! Здесь ты можешь анонимно задать вопрос интересующему тебя человеку! ");
                    await botClient.SendTextMessageAsync(chatId, "Чтобы задать вопрос нажми на кнопку ниже) ", replyMarkup: inlineKeyboard);
                    session.Asked = true;
                }

                if (session.Send)
                {
                    await botClient.SendTextMessageAsync(chatId, "Твой вопрос отправлен!\n\nЧтобы спросить еще что-нибудь нажми на копку ниже", replyMarkup: inlineKeyboard);
                    await botClient.SendTextMessageAsync(long.Parse(session.Key), "Привет! \nУ тебя новый анонимный вопрос 💬 \n\n" + message.Text);
                    var text = $"Начните получать анонимные вопросы прямо сейчас! \n \n👉 t.me/perviy111bot?start={message.Chat.Id} \n \nРазместите эту ссылку ☝️ в описании своего профиля Telegram, TikTok, Instagram (stories), чтобы вам могли написать 💬";
                    await botClient.SendTextMessageAsync(chatId, text);
                    session.Send = false;
                }
            }
        }


        async static Task Error(ITelegramBotClient client, Exception exception, CancellationToken token)
        {
            return;
        }
    }


}
