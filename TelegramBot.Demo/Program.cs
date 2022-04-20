using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramBot.Demo
{
    public class Program
    {
        private static readonly string channelId = "-1001716146422";
        private static readonly TelegramBotClient botClient = new TelegramBotClient("5114181897:AAGjTF5s4oKxfTcubnkbzfifplDNq5RampA");
        private static readonly CancellationTokenSource cts = new CancellationTokenSource();
        private static readonly Update update;
        static Task Main(string[] args)
        {
            Program program = new();
            return program.MainTest(args);
        }

        public async Task MainTest(string[] args)
        {
            try
            {
                //var message = "testtttttttttt";
                //await botClient.SendTextMessageAsync(
                //    chatId: chatId,
                //    text: message);

                //var me = await botClient.GetMeAsync();
                await FirstChat();

                Console.WriteLine();
                Console.WriteLine("######## End ##########");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ex: " + ex.ToString());
            }

            Console.ReadKey();
        }

        public async Task BotInfo()
        {
            var me = await botClient.GetMeAsync();
            Console.WriteLine($"Hello, World! I am user {me.Id} and my name is {me.FirstName}.");
        }

        #region First chat box
        public async Task FirstChat()
        {
            // StartReceiving does not block the caller thread. Receiving is done on the ThreadPool.
            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = { } // receive all update types
            };
            botClient.StartReceiving(
                HandleUpdateAsync,
                HandleErrorAsync,
                receiverOptions,
                cancellationToken: cts.Token);

            var me = await botClient.GetMeAsync();

            Console.WriteLine($"Start listening for @{me.Username}");
            Console.ReadLine();

            // Send cancellation request to stop bot
            cts.Cancel();
        }
        async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            // Only process Message updates: https://core.telegram.org/bots/api#message
            Console.WriteLine($"Message: {UpdateType.Message}");

            if (update.Type != UpdateType.Message)
                return;

            // Only process text and contact messages
            Console.WriteLine($"Text: {MessageType.Text} - update.Message!.Type: {update.Message!.Type}");
            //if (update.Message!.Type != MessageType.Text || update.Message!.Type != MessageType.Contact)
            //    return;

            var chatId = update.Message.Chat.Id;
            var messageText = update.Message.Text;
            var phone = update.Message.Contact?.PhoneNumber;

            Console.WriteLine($"Received a '{messageText}' message in chat {chatId} phone {phone}");

            //// Echo received message text
            //await botClient.SendTextMessageAsync(
            //    chatId: chatId,
            //    text: "You said:\n" + messageText,
            //    cancellationToken: cancellationToken);

            //await botClient.SendTextMessageAsync(
            //    chatId: chatId,
            //    text: "Trying *all the parameters* of `sendMessage` method",
            //    parseMode: ParseMode.MarkdownV2,
            //    disableNotification: true,
            //    replyToMessageId: update.Message.MessageId,
            //    replyMarkup: new InlineKeyboardMarkup(
            //        InlineKeyboardButton.WithUrl(
            //            "Check sendMessage method",
            //            "https://core.telegram.org/bots/api#sendmessage")),
            //    cancellationToken: cancellationToken);

            await ReplyMarkup(chatId, update);
        }

        Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException
                    => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            Console.WriteLine(ErrorMessage);
            return Task.CompletedTask;
        }
        #endregion

        #region Sending message
        public async Task SendMessage()
        {
            // Text
            await botClient.SendTextMessageAsync(
                chatId: channelId,
                text: "Hello, World!",
                cancellationToken: cts.Token);

            // Sticker
            await botClient.SendStickerAsync(
                chatId: channelId,
                sticker: "https://github.com/TelegramBots/book/raw/master/src/docs/sticker-dali.webp",
                cancellationToken: cts.Token);

            // Video
            await botClient.SendVideoAsync(
            chatId: channelId,
            video: "https://github.com/TelegramBots/book/raw/master/src/docs/video-bulb.mp4",
            cancellationToken: cts.Token);
        }
        #endregion

        #region Reply Markup
        public async Task ReplyMarkup(long chatId, Update update)
        {
            //// Single row
            //ReplyKeyboardMarkup replyKeyboardMarkup = new(new[]
            //    {
            //        new KeyboardButton[] { "Help me", "Call me ☎️" },
            //    })
            //    {
            //        ResizeKeyboard = true
            //    };

            //await botClient.SendTextMessageAsync(
            //    chatId: chatId,
            //    text: "Choose a response",
            //    replyMarkup: replyKeyboardMarkup,
            //    cancellationToken: cts.Token);


            //// Multi row
            //ReplyKeyboardMarkup replyKeyboardMarkup = new(new[]
            //    {
            //        new KeyboardButton[] { "One", "Two" },
            //        new KeyboardButton[] { "Three", "Four" },
            //    })
            //    {
            //        ResizeKeyboard = true
            //    };

            //await botClient.SendTextMessageAsync(
            //    chatId: chatId,
            //    text: "Choose a response",
            //    replyMarkup: replyKeyboardMarkup,
            //    cancellationToken: cts.Token);


            // Request information
            ReplyKeyboardMarkup replyKeyboardMarkup = new(new[]
                {
                    KeyboardButton.WithRequestLocation("Share Location"),
                    KeyboardButton.WithRequestContact("Share Contact ☎️"),
                })
            { ResizeKeyboard = true };

            var contact = update.Message.Contact;
            //Console.WriteLine("Contact: ", contact?.PhoneNumber);
            if (contact?.PhoneNumber == null)
            {
                Message sentMessage = await botClient.SendTextMessageAsync(
                   chatId: chatId,
                   text: "Who or Where are you?",
                   replyMarkup: replyKeyboardMarkup,
                   cancellationToken: cts.Token);

            }
            else
            {
                await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: $"Your contact Phone: {contact.PhoneNumber} FirstName: {contact.FirstName} LastName: {contact.LastName}",
                    cancellationToken: cts.Token);
            }

            _ = botClient.GetUpdatesAsync();

            ////Callback buttons
            //InlineKeyboardMarkup inlineKeyboard = new(new[]
            //{
            //    // first row
            //    new []
            //    {
            //        InlineKeyboardButton.WithCallbackData(text: "1.1", callbackData: "11"),
            //        InlineKeyboardButton.WithCallbackData(text: "1.2", callbackData: "12"),
            //    },
            //    // second row
            //    new []
            //    {
            //        InlineKeyboardButton.WithCallbackData(text: "2.1", callbackData: "21"),
            //        InlineKeyboardButton.WithCallbackData(text: "2.2", callbackData: "22"),
            //    },
            //});

            //Message sentMessage = await botClient.SendTextMessageAsync(
            //    chatId: chatId,
            //    text: "A message with an inline keyboard markup",
            //    replyMarkup: inlineKeyboard,
            //    cancellationToken: cts.Token);

            ////URL buttons
            //            InlineKeyboardMarkup inlineKeyboard = new(new[]
            //    {
            //        InlineKeyboardButton.WithUrl(
            //            text: "Link to the Repository",
            //            url: "https://github.com/TelegramBots/Telegram.Bot"
            //        )
            //    }
            //);

            //            Message sentMessage = await botClient.SendTextMessageAsync(
            //                chatId: chatId,
            //                text: "A message with an inline keyboard markup",
            //                replyMarkup: inlineKeyboard,
            //                cancellationToken: cts.Token);

            ////Switch to Inline buttons
            //            InlineKeyboardMarkup inlineKeyboard = new(new[]
            //    {
            //        InlineKeyboardButton.WithSwitchInlineQuery("switch_inline_query"),
            //        InlineKeyboardButton.WithSwitchInlineQueryCurrentChat("switch_inline_query_current_chat"),
            //    }
            //);

            //            Message sentMessage = await botClient.SendTextMessageAsync(
            //                chatId: chatId,
            //                text: "A message with an inline keyboard markup",
            //                replyMarkup: inlineKeyboard,
            //                cancellationToken: cts.Token);
        }
        #endregion
    }
}
