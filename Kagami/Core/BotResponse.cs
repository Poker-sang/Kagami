using Kagami.ArgTypes;
using Kagami.UsedTypes;
using Konata.Core;
using Konata.Core.Events.Model;
using Konata.Core.Message;
using Konata.Core.Message.Model;
using System.Reflection;
using System.Text;

namespace Kagami.Core;

internal static class BotResponse
{
    /// <summary>
    /// 通过反射获取所有可用触发
    /// </summary>
    internal static List<Record<TriggerAttribute>> Triggers { get; } = new();

    /// <summary>
    /// 通过反射获取所有可用命令
    /// </summary>
    internal static Dictionary<CmdletType, HashSet<Record<CmdletAttribute>>> Cmdlets { get; }

    /// <summary>
    /// 第一项为好友处理消息数<br/>
    /// 之后为 &lt;群号, 处理消息数&gt;
    /// </summary>
    public static Dictionary<uint, int> MessageCounter { get; }
        = new() { { 0, 0 } };

    internal static bool AllowEcho;

    static BotResponse()
    {
        var types = AppDomain
            .CurrentDomain
            .GetAssemblies()
            .SelectMany(asm => asm.GetTypes())
            // static 的修饰符是 abstract sealed
            .Where(t => t.Namespace is { } ns && (ns.Contains("Kagami.Commands") || ns.Contains("Kagami.Triggers") || ns.Contains("Kagami.Core")))
            .Where(t => t.IsAbstract && t.IsSealed);

        var cmdlet = new List<Record<CmdletAttribute>>();
        foreach (var type in types)
        {
            var tempCmdlet = new List<Record<CmdletAttribute>>();
            var tempTriggers = new List<Record<TriggerAttribute>>();
            foreach (var method in type.GetMethods())
                // 没有标注是命令的
                if (method.GetCustomAttribute<CmdletAttribute>() is { } cmdletAttribute)
                {
                    if (CommandParser.Get<MessageBuilder>(method, cmdletAttribute) is { } c)
                        tempCmdlet.Add(c);
                }
                else if (method.GetCustomAttribute<TriggerAttribute>() is { } triggerAttribute)
                {
                    if (TriggerParser.Get<bool>(method, triggerAttribute) is { } t)
                        tempTriggers.Add(t);
                }

            cmdlet.AddRange(tempCmdlet);
            Triggers.AddRange(tempTriggers);
        }

        Cmdlets = cmdlet.GroupBy(i => i.Attribute.CmdletType)
            .ToDictionary(
            i => i.Key,
            i => new HashSet<Record<CmdletAttribute>>(i));
        Triggers.Sort((a, b) => a.Attribute.TriggerPriority.CompareTo(b.Attribute.TriggerPriority));
    }

    /// <summary>
    /// 给机器人挂事件的入口
    /// </summary>
    /// <param name="bot"></param>
    /// <param name="group"></param>
    /// <returns></returns>
    public static async void Entry(Bot bot, GroupMessageEvent group)
    {
        Console.WriteLine($"[\x1b[38;2;0;255;255m{DateTime.Now:T}\u001b[0m] [\x1b[38;2;0;0;255m{group.GroupName}/\x1b[38;2;0;255;0m{group.MemberCard.Replace("\x7", "")}\u001b[0m]: {group.Chain}\u001b[0m");

        if (!AllowEcho && group.MemberUin == bot.Uin)
            return;

        if (group.MemberUin == bot.Uin)
            AllowEcho = false;

        if (!MessageCounter.ContainsKey(group.GroupUin))
            MessageCounter[group.GroupUin] = 0;

        ++MessageCounter[group.GroupUin];

        if (group.Message.Chain is { Count: 0 })
            return;

        var sb = new StringBuilder();
        foreach (var chain in group.Message.Chain)
            _ = sb.Append(chain switch
            {
                TextChain textChain => textChain.Content,
                _ => @$" '<placeholder type=""{chain.Type}""/>' "
            });

        var raw = new Raw(sb.ToString().Trim());

        // 忽略空消息
        if (raw.RawString is "")
            return;

        try
        {
            // 弃元：是否处理了消息
            _ = await TriggerParser.Process(bot, group, raw);
        }
        catch (Exception e)
        {
            await Console.Error.WriteLineAsync(e.Message);
        }
    }
}
