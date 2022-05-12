using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Konata.Core.Message;
using Konata.Core.Message.Model;

namespace Kagami;

/// <summary>
/// Kagami 命令接口
/// </summary>
/// <remarks>
/// 表示这是一个可用的命令
/// </remarks>
public interface IKagamiCommand
{
    /// <summary>
    /// 命令的标识
    /// </summary>
    string Command { get; }

    /// <summary>
    /// 命令的友好名称
    /// </summary>
    string Name => Command;

    /// <summary>
    /// 命令的说明
    /// </summary>
    string Description { get; }

    /// <summary>
    /// 命令的参数列表
    /// </summary>
    ValueTuple<Type, string>[] Arguments { get; }

    /// <summary>
    /// 命令需要的最少参数
    /// </summary>
    int ArgumentCount => Arguments.Length;

    /// <summary>
    /// 执行命令
    /// </summary>
    /// <param name="args">参数</param>
    /// <returns><seealso cref="MessageBuilder"/></returns>
    Task<MessageBuilder> InvokeAsync(Konata.Core.Bot? bot, Konata.Core.Events.Model.GroupMessageEvent? group, object[] args);

    /// <summary>
    /// 命令的激活方式
    /// </summary>
    CommandType CommandType => CommandType.Normal;

    /// <summary>
    /// 是否不区分大小写
    /// </summary>
    bool IgnoreCase => true;
}
