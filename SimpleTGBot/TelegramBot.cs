namespace SimpleTGBot;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Newtonsoft.Json;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;
using System;

public class TelegramBot
{
    // Токен TG-бота. Можно получить у @BotFather
    private const string BotToken = "6712913477:AAF1ZpNugQ0Zn2eezVmc1bj4UyX7qn7TCak";
    // Open weather map API key: 3b500f012db38b659c0b1761d88cdc11
    string APIk = "3b500f012db38b659c0b1761d88cdc11";

    /// <summary>
    /// Инициализирует и обеспечивает работу бота до нажатия клавиши Esc
    /// </summary>
    public async Task Run()
    {
        // Если вам нужно хранить какие-то данные во время работы бота (массив информации, логи бота,
        // историю сообщений для каждого пользователя), то это всё надо инициализировать в этом методе.
            using StreamWriter stream = new StreamWriter("WeatherBotLogs.txt");
            {
                stream.WriteLine("@Maxg1fWeatherBot(https://t.me/Maxg1fWeatherBot) bot started. \n");
                stream.Close();
            }
        // TODO: Инициализация необходимых полей
        
        // Инициализируем наш клиент, передавая ему токен.
        var botClient = new TelegramBotClient(BotToken);
        
        // Служебные вещи для организации правильной работы с потоками
        using CancellationTokenSource cts = new CancellationTokenSource();
        // Разрешённые события, которые будет получать и обрабатывать наш бот.
        // Будем получать только сообщения. При желании можно поработать с другими событиями.
        ReceiverOptions receiverOptions = new ReceiverOptions()
        {
            AllowedUpdates = new [] { UpdateType.Message }
        };

        // Привязываем все обработчики и начинаем принимать сообщения для бота
        botClient.StartReceiving(
            updateHandler: OnMessageReceived,
            pollingErrorHandler: OnErrorOccured,
            receiverOptions: receiverOptions,
            cancellationToken: cts.Token
        );

        // Проверяем что токен верный и получаем информацию о боте
        var me = await botClient.GetMeAsync(cancellationToken: cts.Token);
        Console.WriteLine($"Бот @{me.Username} запущен.\nДля остановки нажмите клавишу Esc..."); //1899572625

        // Ждём, пока будет нажата клавиша Esc, тогда завершаем работу бота
        while (Console.ReadKey().Key != ConsoleKey.Escape){}

        // Отправляем запрос для остановки работы клиента.
        cts.Cancel();
    }

    string ws = "now";
    string st = "city";
    /// <summary>
    /// Обработчик события получения сообщения.
    /// </summary>
    /// <param name="botClient">Клиент, который получил сообщение</param>
    /// <param name="update">Событие, произошедшее в чате. Новое сообщение, голос в опросе, исключение из чата и т. д.</param>
    /// <param name="cancellationToken">Служебный токен для работы с многопоточностью</param>
    async Task OnMessageReceived(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        // Работаем только с сообщениями. Остальные события игнорируем
        var message = update.Message;
        if (message is null)
        {
            return;
        }
        // Будем обрабатывать только текстовые сообщения.
        // При желании можно обрабатывать стикеры, фото, голосовые и т. д.
        //
        // Обратите внимание на использованную конструкцию. Она эквивалентна проверке на null, приведённой выше.
        // Подробнее об этом синтаксисе: https://medium.com/@mattkenefick/snippets-in-c-more-ways-to-check-for-null-4eb735594c09
        if (message.Text is not { } messageText)
        {
            return;
        }

        if (update.Type == UpdateType.CallbackQuery)
        {

        }
        // Получаем ID чата, в которое пришло сообщение. Полезно, чтобы отличать пользователей друг от друга.
        var chatId = message.Chat.Id;
        
        // Печатаем на консоль факт получения сообщения
        Console.WriteLine($"Получено сообщение в чате {chatId}: '{messageText}'");
        using StreamWriter stream = new StreamWriter("WeatherBotLogs.txt");
        stream.WriteLine($"Получено сообщение в чате {chatId}: '{messageText}'");


        string botmessage = "Error";
        string lat = "";
        string lon = "";

        //Получаем погоду с помощью api
        void GetWeatherNow()
        {

            if (st == "coord")
            {
                var r = Regex.Match(messageText, @"(?<lat>[0-9]{1,3}\.[0-9]{1,});(?<lon>[0-9]{1,3}\.[0-9]{1,})");
                lat = r.Groups["lat"].Value;
                lon = r.Groups["lon"].Value;
            }

            using (WebClient wc = new WebClient())
            {
                string url = "";
                if (ws == "now")
                {
                    if (st == "city")
                        url = string.Format("https://api.openweathermap.org/data/2.5/weather?q={0}&appid={1}&units=metric&lang=ru", messageText, APIk);
                    else
                        url = string.Format("https://api.openweathermap.org/data/2.5/weather?lat={0}&lon={1}&appid={2}&units=metric&lang=ru", lat, lon, APIk);
                    
                    var json = wc.DownloadString(url);
                    WeatherInfo.now data = JsonConvert.DeserializeObject<WeatherInfo.now>(json);

                    botmessage = $"Погода в {messageText} сейчас: \n {data.main.temp}°C чувствуется как: {data.main.feels_like}°C \n {data.weather[0].main}: {data.weather[0].description} \n Скорость ветра:{data.wind.speed} м/c, Атмосфеное давление: {data.main.pressure * 0.75} мм. рт. ст. \n Восход:{data.sys.sunrise}, Закат: {data.sys.sunset}";
                }
                else if (ws == "1d/3h")
                {
                    if (st == "city")
                        url = string.Format("https://api.openweathermap.org/data/2.5/forecast?q={0}&appid={1}&cnt=8&units=metric&lang=ru", message.Text, APIk);
                    else
                        url = string.Format("https://api.openweathermap.org/data/2.5/weather?lat={0}&lon={1}&appid={2}&cnt=8&units=metric&lang=ru", lat, lon, APIk);
                    
                    var json = wc.DownloadString(url);
                    WeatherInfo.day data = JsonConvert.DeserializeObject<WeatherInfo.day>(json);

                    botmessage = "";
                    foreach (var item in data.list)
                        botmessage += $"Погода в {messageText} {item.dt_txt}:\n {item.main.temp}°C чувствуется как: {item.main.feels_like}°C \n {item.weather[0].main}: {item.weather[0].description} \n Скорость ветра:{item.wind.speed} м/c, Атмосфеное давление: {item.main.pressure * 0.75} мм. рт. ст. \n";
                    
                    botmessage += $"Восход: {data.sunrize}, Закат: //{data.sunset}";//{data.sunrize} {data.sunset}
                }
                else
                //Задел на будущее
                Console.WriteLine("Такого ещё нет");
            }
        }

        ReplyKeyboardMarkup RKM = new(new[]
        {
        new[]
        {
            new KeyboardButton("настройки"),
        },
        new[]
        {
            new KeyboardButton("погода сейчас"),
            new KeyboardButton("погода 1д/3ч"),
        },
        new[]
        {
            new KeyboardButton("погода по городу"),
            new KeyboardButton("погода по координатам"),
        }})
        { ResizeKeyboard = true};

        if (messageText.ToLower() == "погода сейчас")
        {
            botmessage = "Показываем погоду на данный момент.";
            ws = "now";
        }
        else if (messageText.ToLower() == "погода 1д/3ч")
        {
            botmessage = "Показываем погоду на 24ч отрезками по 3 часа.";
            ws = "1d/3h";
        }
        else if (messageText.ToLower() == "погода по городу")
        {
            botmessage = "Показываем погоду по названию города(менее точный чем по координатам).";
            st = "city";
        }
        else if (messageText.ToLower() == "погода по координатам")
        {
            botmessage = "Показываем погоду по координатам.";
            st = "coord";
        }
        else st = st; ws = ws ;

        if (messageText.ToLower() == "/start" || messageText.ToLower() == "настройки")
        {
            //Не получилось ;(. Работают только ссылки
            CallbackQuery callbackQuery = new CallbackQuery();

            InlineKeyboardMarkup IKM = new(new[]
            {
            new[]
            {
            InlineKeyboardButton.WithCallbackData(text: "Погода сейчас", callbackData: "now"),
            InlineKeyboardButton.WithCallbackData(text: "Погода 1д/3ч", callbackData: "1d/3h"),
            },
            new[]
            {
            InlineKeyboardButton.WithCallbackData(text: "Погода по городу", callbackData: "city"),
            InlineKeyboardButton.WithCallbackData(text: "Погода по координатам", callbackData: "coord"),
            },
            new[]
            {
            InlineKeyboardButton.WithUrl(text: "Link to the Repository",
            url: "https://github.com/MMCS-FIIT/cs211a-bonus2-Max-g1f"),
            InlineKeyboardButton.WithUrl(
            text: "Link to README",
            url: "https://github.com/MMCS-FIIT/cs211a-bonus2-Max-g1f/blob/main/WeatherBot_README.md")
            }
            });

            Message sentMessage = await botClient.SendTextMessageAsync(
            chatId: chatId,
            text: "Настройка бота",
            replyMarkup: IKM,
            disableNotification: true,
            replyToMessageId: update.Message.MessageId,
            cancellationToken: cancellationToken);
            stream.WriteLine($"Ответ бота: {sentMessage}") ;
        }
        else if ((messageText.ToLower() == "погода сейчас") || (messageText.ToLower() == "погода 1д/3ч") || (messageText.ToLower() == "погода по городу") || (messageText.ToLower() == "погода по координатам"))
        {
            Message sentMessage = await botClient.SendTextMessageAsync(
            chatId: chatId,
            text: botmessage,
            replyMarkup: RKM,
            disableNotification: true,
            replyToMessageId: update.Message.MessageId,
            cancellationToken: cancellationToken);
            stream.WriteLine($"Ответ бота: {sentMessage}");
        }
        else
        {
            try
            { GetWeatherNow();}
            catch (Exception ex) { Console.WriteLine(ex.Message); botmessage = "Ошибка! Попробуйте ввести другие данные."; }
            Message sentMessage = await botClient.SendTextMessageAsync(
            chatId: chatId,
            text: botmessage,
            replyMarkup: RKM,
            disableNotification: true,
            replyToMessageId: update.Message.MessageId,
            cancellationToken: cancellationToken);
            stream.WriteLine($"Ответ бота: {sentMessage}");
        }
        stream.Close();
    }

    /// <summary>
    /// Обработчик исключений, возникших при работе бота
    /// </summary>
    /// <param name="botClient">Клиент, для которого возникло исключение</param>
    /// <param name="exception">Возникшее исключение</param>
    /// <param name="cancellationToken">Служебный токен для работы с многопоточностью</param>
    /// <returns></returns>
    Task OnErrorOccured(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        // В зависимости от типа исключения печатаем различные сообщения об ошибке
        var errorMessage = exception switch
        {
            ApiRequestException apiRequestException
                => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            
            _ => exception.ToString()
        };

        Console.WriteLine(errorMessage);
        
        // Завершаем работу
        return Task.CompletedTask;
    }
}