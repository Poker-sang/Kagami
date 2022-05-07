using System.IO;
using System.Threading.Tasks;
using Kagami.Attributes;
using Konata.Core;
using Konata.Core.Events.Model;
using Konata.Core.Interfaces.Api;
using Konata.Core.Message;

namespace Kagami.Function;

public static partial class Commands
{
    private static MessageStruct? _lastMessage;

    [Help("撤回我的上条消息")]
    private static async Task<MessageBuilder?> Recall(Bot bot, GroupMessageEvent group)
    {
        if (_lastMessage is not null)
            _ = await bot.RecallMessage(_lastMessage);
        
        return null;
    }
}