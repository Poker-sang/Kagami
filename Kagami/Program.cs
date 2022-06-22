using Kagami.Core;
using Kagami.Services;
using Kagami.Utilities;
using Konata.Core;
using Konata.Core.Common;
using Konata.Core.Events.Model;
using Konata.Core.Interfaces;
using Konata.Core.Interfaces.Api;
using System.Diagnostics;
using System.Text.Json;

namespace Kagami;

public static class Program
{
    private static Bot bot = null!;

    public static async Task Main()
    {
        Console.WriteLine("Starting...");

        _ = Task.Run(HttpClientExtensions.Initialize);

        bot = BotFather.Create(GetConfig(), GetDevice(), GetKeyStore());

        Retransmit.TryLoad();

        // Print the log
        bot.OnLog += (_, e) => Trace.WriteLine(e.EventMessage);

        bot.OnBotOnline += (_, e) => Console.WriteLine(e.EventMessage);

        // Handle the captcha
        bot.OnCaptcha += (s, e) =>
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
        bot.OnGroupPoke += Poke.OnGroupPoke;

        // Handle messages from group
        bot.OnGroupMessage += BotResponse.Entry;

        // Retransmit messages
        bot.OnFriendMessage += Retransmit.OnFriendMessage;

        // Login the bot
        var result = await bot.Login();
        // Update the keystore
        if (result)
            _ = UpdateKeystore(bot.KeyStore);

        Console.WriteLine("Running...");

        // cli
        try
        {
            while (true)
            {
                var message = Console.ReadLine() ?? "";
                var args = message.SplitRawString();
                if (args.Length is 0)
                    continue;
                switch (args[0])
                {
                    case "/stop":
                        return;
                    case "/echo":
                        BotResponse.AllowEcho = true;
                        break;
                    case "/help":
                        _ = Help.GenerateImageAsync(true);
                        break;
                    case "/retransmit":
                        if (args.Length > 2 && uint.TryParse(args[1], out var friendUin) && uint.TryParse(args[2], out var groupUin))
                        {
                            Retransmit.FriendUin = friendUin;
                            Retransmit.GroupUin = groupUin;
                            Retransmit.Save();
                        }
                        Console.WriteLine($"[Now]: retransmitting friend {Retransmit.FriendUin} to group {Retransmit.GroupUin}");
                        break;
                    case "/relogin":
                        File.Delete("keystore.json");
                        return;
                    default:
                        break;
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
        finally
        {
            _ = await bot.Logout();
            bot.Dispose();
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
        HighwayChunkSize = 2 << 12,
    };

    /// <summary>
    /// Load or create device 
    /// </summary>
    /// <returns></returns>
    private static BotDevice GetDevice()
    {
        // Read the device from config
        if (File.Exists("device.json") && JsonSerializer.Deserialize
                <BotDevice>(File.ReadAllText("device.json")) is { } device)
            return device;

        // Create new one
        device = BotDevice.Default();

        var deviceJson = JsonSerializer.Serialize(device,
            new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText("device.json", deviceJson);

        return device;
    }

    /// <summary>
    /// Load or create keystore
    /// </summary>
    /// <returns></returns>
    private static BotKeyStore GetKeyStore()
    {
        // Read the device from config

        if (File.Exists("keystore.json") && JsonSerializer.Deserialize
                <BotKeyStore>(File.ReadAllText("keystore.json")) is { } key)
            return key;

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
