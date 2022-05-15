using Kagami.Utils;
using Konata.Core;
using Konata.Core.Events.Model;
using Konata.Core.Interfaces.Api;
using Konata.Core.Message;
using Konata.Core.Message.Model;
using System.Text;

namespace Kagami;

/// <summary>
/// 入口
/// </summary>
public static class Entry
{
    /// <summary>
    /// 通过反射获取所有可用命令
    /// </summary>
    public static Dictionary<CommandType, HashSet<IKagamiCmdlet>> Commands { get; }
        = AppDomain
            .CurrentDomain
            .GetAssemblies()
            .SelectMany(asm => asm.GetTypes())
            .Where(t => t.GetInterfaces().Contains(typeof(IKagamiCmdlet)))
            .Select(t => (IKagamiCmdlet)Activator.CreateInstance(t)!)
            .GroupBy(o => o.CommandType)
            .ToDictionary(
                group => group.Key,
                group => new HashSet<IKagamiCmdlet>(group));

    /// <summary>
    /// 给机器人挂事件的入口
    /// </summary>
    /// <param name="bot"></param>
    /// <param name="group"></param>
    /// <returns></returns>
    public static async void ParseCommand(Bot bot, GroupMessageEvent group)
    {
        var value = group.Message.Chain[0] is TextChain textChain ? await ParseCommand(textChain.Content, bot, group) : null;
        if (value is not null)
            _ = await bot.SendGroupMessage(group.GroupUin, value);
    }

    /// <summary>
    /// 解析命令
    /// </summary>
    /// <param name="raw">原始字符串</param>
    /// <returns></returns>
    public static async Task<MessageBuilder?> ParseCommand(string raw, Bot bot, GroupMessageEvent group)
    {
        if (string.IsNullOrWhiteSpace(raw))
            throw new ArgumentException($"“{nameof(raw)}”不能为 null 或空白。", nameof(raw));

        string[] args = SplitCommand(raw);
        string cmd = args[0]; // 获取第一个元素用作命令

        if (Commands.TryGetValue(CommandType.Normal, out var set))
            return await ParseNormalCommand(cmd, args, set, bot, group);
        else if (Commands.TryGetValue(CommandType.Prefix, out set))
            return await ParsePrefixCommand(cmd, args, set, bot, group);
        else
            return null;
    }

    /// <summary>
    /// 执行命令
    /// </summary>
    /// <param name="command">命令</param>
    /// <param name="args">参数</param>
    /// <exception cref="ArgumentException">断言工具抛出</exception>
    /// <exception cref="KeyNotFoundException">当类型检查器不存在时抛出此异常</exception>
    /// <returns></returns>
    private static async Task<MessageBuilder?> InvokeCommand(IKagamiCmdlet command, string[] args, Bot bot, GroupMessageEvent group)
    {
        try
        {
            return await command.InvokeAsync(bot, group, ParseArguments(command, args, group));
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex);
            return new MessageBuilder(ex.GetType().FullName)
                .TextLine(ex.Message);
        }
    }

    private static object[] ParseArguments(IKagamiCmdlet command, string[] args, GroupMessageEvent group)
    {
        Assert.ThrowIfNot<ArgumentException>(args.Length >= command.ArgumentCount, "不满足最少所需要的参数数量");

        var tArgs = new object[args.Length];

        for (int j = 0; j < command.OverloadableArgumentList.Length; ++j)
        {
            var ArgumentList = command.OverloadableArgumentList[j];
            bool pass = true;
            for (int i = 0; i < ArgumentList.Length && i < args.Length; ++i)
            {
                (Type Type, string Description) = ArgumentList[i];
                if (!TypeCheck.Map[Type](in args[i], out object? obj, in group))
                {
                    pass = false;
                    break;
                }
                tArgs[i] = obj;
            }

            if (pass)
                return tArgs;
        }

        throw new InvalidOperationException("找不到合适的cmdlet重载");
    }

    /// <summary>
    /// 解析前缀命令
    /// </summary>
    /// <param name="sz_cmd">命令名</param>
    /// <param name="args">命令参数</param>
    /// <param name="set">命令字典</param>
    /// <returns></returns>
    private static async Task<MessageBuilder?> ParseNormalCommand(string sz_cmd, string[] args, HashSet<IKagamiCmdlet> set, Bot bot, GroupMessageEvent group)
    {
        if (string.IsNullOrEmpty(sz_cmd))
            throw new ArgumentException($"“{nameof(sz_cmd)}”不能为 null 或空。", nameof(sz_cmd));

        // // 匹配命令
        // if (!map.TryGetValue(cmd.ToLower(), out var command)
        //     || !command.Command.Equals(cmd, command.IgnoreCase
        //     ? StringComparison.OrdinalIgnoreCase
        //     : StringComparison.Ordinal))
        //     return null; // 找不到匹配的命令
        // 为什么不这样写?
        // 因为如果这样写, 我们就需要在这里把命令名转换为小写, 但是这样做会导致命令名无法匹配到正确的命令
        // 举个例子: 有一个命令, 叫做"CMD", 且配置为不区分大小写, 此时我们输入
        // "cmd", "cMd" 或其他大小写混搭的格式就会不能匹配到正确的命令.

        // 匹配命令
        if (set.FirstOrDefault(cmd => sz_cmd.Equals(cmd.Command, cmd.IgnoreCase
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal)) is not IKagamiCmdlet command)
            return null; // 找不到匹配的命令

        args = args.Skip(1).ToArray(); // 跳过用于表示命令的第一个元素

        return await InvokeCommand(command, args, bot, group);
    }

    /// <summary>
    /// 解析前缀命令
    /// </summary>
    /// <param name="sz_cmd"> 命令名 </param>
    /// <param name="args"> 命令参数 </param>
    /// <param name="set"> 命令字典 </param>
    /// <returns> </returns>
    private static async Task<MessageBuilder?> ParsePrefixCommand(string sz_cmd, string[] args, HashSet<IKagamiCmdlet> set, Bot bot, GroupMessageEvent group)
    {
        if (string.IsNullOrEmpty(sz_cmd))
            throw new ArgumentException($"“{nameof(sz_cmd)}”不能为 null 或空。", nameof(sz_cmd));

        // 匹配命令
        if (set.FirstOrDefault(cmd => sz_cmd.StartsWith(cmd.Command, cmd.IgnoreCase
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal)) is not IKagamiCmdlet command)
            return null; // 找不到匹配的命令

        args[0] = sz_cmd[command.Command.Length..]; // 首个参数需要跳过前缀

        return await InvokeCommand(command, args, bot, group);
    }

    /// <summary>
    /// 先进字符串分割
    /// </summary>
    /// <param name="raw">原始字符串</param>
    /// <returns>分割后的字符串数组</returns>
    /// <exception cref="FormatException">引号栈不平衡时引发的异常</exception>
    private static string[] SplitCommand(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            throw new ArgumentException($"“{nameof(raw)}”不能为 null 或空白。", nameof(raw));

        List<string> result = new();
        StringBuilder sb = new();
        Stack<char> quotes = new();
        foreach (var ch in raw)
        {
            switch (ch)
            {
                case '"':
                case '\'':
                    if (quotes.TryPeek(out var tmp) && tmp == ch)
                        quotes.Pop();
                    else
                        quotes.Push(ch);
                    break;

                case ' ':
                    if (quotes.Count is 0)
                    {
                        if (sb.Length > 0)
                            result.Add(sb.ToString());
                        sb.Clear();
                    }
                    else
                    {
                        sb.Append(ch);
                    }
                    break;

                default:
                    sb.Append(ch);
                    break;
            }
        }
        Assert.ThrowIfNot<FormatException>(quotes.Count is 0, "输入的格式不正确");

        result.Add(sb.ToString());
        sb.Clear();

        return result.ToArray();
    }
}
