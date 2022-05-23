using Konata.Core;
using Konata.Core.Common;
using Konata.Core.Events.Model;
using Konata.Core.Interfaces;
using Konata.Core.Interfaces.Api;
using System.Diagnostics;
using System.Text.Json;

namespace Kagami.Core;

public static class Program
{
    private static Bot Bot = null!;

    public static async Task Main()
    {
        Console.WriteLine("Starting...");

        Bot = BotFather.Create(GetConfig(), GetDevice(), GetKeyStore());

        // Print the log
        Bot.OnLog += (_, e) => Trace.WriteLine(e.EventMessage);

        Bot.OnBotOnline += (_, e) => Console.WriteLine(e.EventMessage);

        // Handle the captcha
        Bot.OnCaptcha += (s, e) =>
        {
            switch (e.Type)
            {
                case CaptchaEvent.CaptchaType.Sms:
                    Console.WriteLine(e.Phone);
                    _ = s.SubmitSmsCode(Console.ReadLine());
                    break;

                case CaptchaEvent.CaptchaType.Slider:
                    Console.WriteLine(e.SliderUrl);
                    _ = s.SubmitSliderTicket(Console.ReadLine());
                    break;

                default:
                case CaptchaEvent.CaptchaType.Unknown:
                    break;
            }
        };

        // Handle poke messages
        Bot.OnGroupPoke += Services.Poke.OnGroupPoke;

        // Handle messages from group
        Bot.OnGroupMessage += BotResponse.Entry;

        // Login the bot
        var result = await Bot.Login();
        // Update the keystore
        if (result)
            _ = UpdateKeystore(Bot.KeyStore);

        Console.WriteLine("Running...");

        // cli
        var isGroup = false;
        uint uid = 0;
        while (true)
        {
            try
            {
                var message = Console.ReadLine() ?? "";
                var args = ParserUtilities.SplitRawString(message);
                if (args.Length is 0)
                    continue;
                switch (args[0])
                {
                    case "/stop":
                        _ = await Bot.Logout();
                        Bot.Dispose();
                        return;
                    case "/echo":
                        BotResponse.AllowEcho = true;
                        break;
                    case "/refresh":
                        if (args.Length < 2)
                        {
                            Console.WriteLine("[Error]: /refresh <command>");
                            continue;
                        }

                        switch (args[1])
                        {
                            case "help":
                                _ = Services.Help.GenerateImageAsync(true);
                                break;
                            default:
                                break;
                        }

                        break;
                    case "/join":
                        if (args.Length < 2)
                        {
                            Console.WriteLine("[Error]: /join <群号/好友号>");
                            continue;
                        }

                        if (!uint.TryParse(args[1], out uid))
                        {
                            uid = 0;
                            Console.WriteLine("[Error]: /join <群号/好友号>");
                        }

                        goto case "/current";
                    case "/switch":
                        isGroup = !isGroup;
                        goto case "/current";
                    case "/current":
                        Console.WriteLine($"当前是 与{uid}({(isGroup ? "群组" : "好友")}) 聊天");
                        break;
                    default:
                        if (uid is 0)
                        {
                            Console.WriteLine("[Error]: 需要先加入一个群或好友");
                            continue;
                        }

                        _ = isGroup ? Bot.SendGroupMessage(uid, message) : Bot.SendFriendMessage(uid, message);
                        break;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return;
            }
        }
    }

    /// <summary>
    /// Get bot config
    /// </summary>
    /// <returns></returns>
    private static BotConfig GetConfig() => new()
    {
        EnableAudio = true,
        TryReconnect = true,
        HighwayChunkSize = 8192,
    };

    /// <summary>
    /// Load or create device 
    /// </summary>
    /// <returns></returns>
    private static BotDevice? GetDevice()
    {
        // Read the device from config
        if (File.Exists("device.json"))
        {
            return JsonSerializer.Deserialize
                <BotDevice>(File.ReadAllText("device.json"));
        }

        // Create new one
        var device = BotDevice.Default();
        {
            var deviceJson = JsonSerializer.Serialize(device,
                new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText("device.json", deviceJson);
        }

        return device;
    }

    /// <summary>
    /// Load or create keystore
    /// </summary>
    /// <returns></returns>
    private static BotKeyStore? GetKeyStore()
    {
        // Read the device from config
        if (File.Exists("keystore.json"))
            return JsonSerializer.Deserialize
                <BotKeyStore>(File.ReadAllText("keystore.json"));

        Console.WriteLine("For first running, please type your account and password.");

        Console.Write("Account: ");
        var account = Console.ReadLine();

        Console.Write("Password: ");
        var password = Console.ReadLine();

        // Create new one
        Console.WriteLine("Bot created.");
        return UpdateKeystore(new BotKeyStore(account, password));
    }

    /// <summary>
    /// Update keystore
    /// </summary>
    /// <param name="keystore"></param>
    /// <returns></returns>
    private static BotKeyStore UpdateKeystore(BotKeyStore keystore)
    {
        var deviceJson = JsonSerializer.Serialize(keystore,
            new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText("keystore.json", deviceJson);
        return keystore;
    }
}
