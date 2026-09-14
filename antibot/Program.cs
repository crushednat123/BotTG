using Telegram.Bot;
using antibot;

class Program
{
    static async Task Main()
    {
        var bot = new TelegramBotClient("токен");

        // Загружаем админов из файла
        Base.LoadAdmins();

        // Загружаем пользователей из файла
        Base.LoadUsers();

        bot.StartReceiving(
            async (client, update, ct) =>
            {
                // Проверяем, что обновление содержит сообщение
                if (update.Message?.Text == null)
                    return;

                string text = update.Message.Text;
                var sender = update.Message.From;              // Отправитель текущего сообщения
                var chatId = update.Message.Chat.Id;           // ID текущего чата
                var reply = update.Message.ReplyToMessage;     // Сообщение, на которое ответили

                // ------------------ УДАЛЕНИЕ ПОЛЬЗОВАТЕЛЯ И ЕГО СООБЩЕНИЙ ------------------
                if (text != "\\add" && !text.StartsWith("\\role"))
                {
                    if (sender == null)
                        return;

                    string userId = sender.Id.ToString();

                    // Проверяем наличие пользователя в базе ограниченных
                    bool exists = Base.Users.Any(u => u.UserId == userId);

                    if (exists)
                    {
                        try
                        {
                            // 1. Удаляем текущее входящее сообщение
                            await client.DeleteMessage(
                                chatId,
                                update.Message.MessageId,
                                cancellationToken: ct
                            );

                            // 2. Баним пользователя и удаляем все его сообщения
                            await client.BanChatMember(
                                chatId: chatId,
                                userId: sender.Id,
                                revokeMessages: true,
                                cancellationToken: ct
                            );

                            Console.WriteLine(
                                $"Пользователь {sender.Username ?? sender.FirstName} забанен, сообщения удалены."
                            );
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Ошибка при бане/удалении сообщений: " + ex.Message);
                        }

                        return;
                    }

                    return;
                }

                // ------------------ КОМАНДА ROLE ------------------
                if (text.StartsWith("\\role"))
                {
                    if (reply == null)
                    {
                        await client.SendMessage(chatId, "Команда должна быть ответом.");
                        return;
                    }

                    int role = int.Parse(text.Replace("\\role", "").Trim());
                    var target = reply.From;

                    AdminUser admin = new AdminUser
                    {
                        UserId = target.Id.ToString(),
                        UserName = target.Username ?? target.FirstName,
                        Role = role
                    };

                    // Проверяем, что админ ещё не добавлен
                    if (!Base.Admins.Exists(a => a.UserId == admin.UserId))
                    {
                        Base.Admins.Add(admin);
                        Base.SaveAdmins();
                    }

                    await client.SendMessage(chatId, $"Роль {role} назначена пользователю.");
                    return;
                }

                // ------------------ КОМАНДА ADD ------------------
                if (text == "\\add")
                {
                    if (reply == null)
                    {
                        await client.SendMessage(chatId, "Команда должна быть ответом.");
                        return;
                    }

                    bool isAdmin = Base.Admins.Exists(a => a.UserId == sender.Id.ToString());

                    if (!isAdmin)
                    {
                        await client.SendMessage(chatId, "У вас нет прав для выполнения команды.");
                        return;
                    }

                    var target = reply.From;

                    // Проверка на дубли
                    bool exists = Base.Users.Any(u => u.UserId == target.Id.ToString());

                    if (exists)
                    {
                        await client.SendMessage(chatId, "Этот пользователь уже есть в базе.");
                        return;
                    }

                    InfoUser info = new InfoUser
                    {
                        UserId = target.Id.ToString(),
                        UserName = target.Username ?? target.FirstName,
                        Sms = reply.Text,
                        Role = 0
                    };

                    Base.Users.Add(info);
                    Base.SaveUsers();

                    await client.SendMessage(chatId, "Пользователь добавлен в базу.");
                    return;
                }
            },
            (client, exception, ct) =>
            {
                Console.WriteLine(exception);
                return Task.CompletedTask;
            }
        );

        Console.WriteLine("Бот запущен");
        Console.ReadLine();
    }
}
