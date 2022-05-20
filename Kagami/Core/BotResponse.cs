using Kagami.ArgTypes;
using Kagami.Attributes;
using Kagami.Enums;
using Konata.Core;
using Konata.Core.Events.Model;
using Konata.Core.Message.Model;
using System.Reflection;
using System.Text;
using static Konata.Core.Message.BaseChain;

namespace Kagami.Core;

internal static class BotResponse
{
    /// <summary>
    /// 通过反射获取所有可用触发
    /// </summary>
    internal static List<KagamiTrigger> Triggers = new();

    /// <summary>
    /// 通过反射获取所有可用命令
    /// </summary>
    internal static Dictionary<CmdletType, HashSet<KagamiCmdlet>> Cmdlets { get; }

    private static volatile nuint s_messageCounter = 0;
    public static nuint MessageCounter => s_messageCounter;

    static BotResponse()
    {
        var types = AppDomain
            .CurrentDomain
            .GetAssemblies()
            .SelectMany(asm => asm.GetTypes());

        var cmdlets = new List<KagamiCmdlet>();
        foreach (var type in types)
        {
            var tempCmdlets = new List<KagamiCmdlet>();
            var tempTriggers = new List<KagamiTrigger>();
            foreach (var method in type.GetMethods())
                // 没有标注是命令的
                if (method.GetCustomAttribute<KagamiCmdletAttribute>() is { } cmdletAttribute)
                {
                    if (CommandParser.Get(method, cmdletAttribute) is { } cmdlet)
                        tempCmdlets.Add(cmdlet);
                }
                else if (method.GetCustomAttribute<KagamiTriggerAttribute>() is { } triggerAttribute)
                    if (TriggerParser.Get(method, triggerAttribute) is { } trigger)
                        tempTriggers.Add(trigger);

            // 静态类的修饰符是abstract sealed
            // 它不是抽象类
            if ((tempCmdlets.Count is not 0 || tempTriggers.Count is not 0) && !type.IsAbstract)
            {
                var target = Activator.CreateInstance(type);
                for (var i = 0; i < tempCmdlets.Count; ++i)
                    tempCmdlets[i] = tempCmdlets[i] with { Target = target };
                for (var i = 0; i < tempTriggers.Count; ++i)
                    tempTriggers[i] = tempTriggers[i] with { Target = target };
            }

            cmdlets.AddRange(tempCmdlets);
            Triggers.AddRange(tempTriggers);
        }

        Cmdlets = cmdlets.GroupBy(i => i.CommandType)
            .ToDictionary(
            i => i.Key,
            i => new HashSet<KagamiCmdlet>(i));
        Triggers.Sort((a, b) => a.TriggerType.CompareTo(b.TriggerType));
    }

    /// <summary>
    /// 给机器人挂事件的入口
    /// </summary>
    /// <param name="bot"></param>
    /// <param name="group"></param>
    /// <returns></returns>
    public static async void Entry(Bot bot, GroupMessageEvent group)
    {
        Console.WriteLine($"[\x1b[38;2;0;255;255m{DateTime.Now:T}\u001b[0m]  [\x1b[38;2;0;0;255m{group.GroupName}/\x1b[38;2;0;255;0m{group.MemberCard.Replace("\x7", "")}\u001b[0m] : {group.Chain}\u001b[0m");

        if (group.MemberUin == bot.Uin)
            return;

        ++s_messageCounter;

        if (group.Message.Chain is { Count: 0 })
            return;

        var sb = new StringBuilder();
        foreach (var chain in group.Message.Chain)
            _ = sb.Append(chain.Type switch
            {
                ChainType.Text => chain.As<TextChain>()?.Content,
                _ => @$" '<placeholder type=""{chain.Type}""/>' "
            });

        await ProcessRaw(bot, group, sb.ToString().Trim());
    }

    /// <summary>
    /// 开始处理命令
    /// </summary>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static async Task ProcessRaw(Bot bot, GroupMessageEvent group, string raw)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(raw))
                throw new ArgumentException($"\"{nameof(raw)}\" 不能为 null 或空白。", nameof(raw));

            // 弃元：是否处理了消息
            _ = await TriggerParser.Process(bot, group, new Raw(raw));
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e.ToString());
        }
    }
}
