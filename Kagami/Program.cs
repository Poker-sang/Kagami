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
        s_bot = BotFather.Create(GetConfig(), GetDevice(), GetKeyStore());

        // Print the log
        s_bot.OnLog += (_, e) => Console.WriteLine(e.EventMessage);

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
        while (true)
        {
            try
            {
                switch (Console.ReadLine())
                {
                    case @"\stop":
                        _ = await s_bot.Logout();
                        s_bot.Dispose();
                        return;
                }
            }
            catch (Exception)
            {
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
