using System.Diagnostics;
using System.Text.Json;

using Konata.Core;
using Konata.Core.Common;
using Konata.Core.Events.Model;
using Konata.Core.Interfaces;
using Konata.Core.Interfaces.Api;

namespace Kagami;

public static class Program
{
    private static Bot s_bot = null!;

    public static async Task Main()
    {
        Console.WriteLine("Running...");
        s_bot = BotFather.Create(GetConfig(), GetDevice(), GetKeyStore());

        // Print the log
        s_bot.OnLog += (_, e) => Trace.WriteLine(e.EventMessage);

        s_bot.OnBotOnline += (_, e) => Console.WriteLine(e.EventMessage);

        // Handle the captcha
        s_bot.OnCaptcha += (s, e) =>
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
        s_bot.OnGroupPoke += Services.Poke.OnGroupPoke;

        // Handle messages from group
        s_bot.OnGroupMessage += Entry.ParseCommand;

        // Login the bot
        bool result = await s_bot.Login();
        // Update the keystore
        if (result)
            _ = UpdateKeystore(s_bot.KeyStore);

        // cli
        bool isGroup = false;
        uint uid = 0;
        while (true)
        {
            try
            {
                string? message = Console.ReadLine() ?? string.Empty;
                string[] args = Entry.SplitCommand(message);
                if (args.Length is 0)
                    continue;
                switch (args[0])
                {
                    case "/stop":
                        _ = await s_bot.Logout();
                        s_bot.Dispose();
                        return;
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
                        if (isGroup)
                            _ = s_bot.SendGroupMessage(uid, message);
                        else
                            _ = s_bot.SendFriendMessage(uid, message);
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
            string? deviceJson = JsonSerializer.Serialize(device,
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
        string? account = Console.ReadLine();

        Console.Write("Password: ");
        string? password = Console.ReadLine();

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
        string? deviceJson = JsonSerializer.Serialize(keystore,
            new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText("keystore.json", deviceJson);
        return keystore;
    }
}
