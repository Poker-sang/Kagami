namespace Kagami.ArgTypes;

/// <summary>
/// 表示一个回复消息
/// </summary>
/// <param name="Uin"></param>
/// <param name="Sequence"></param>
/// <param name="Uuid"></param>
/// <param name="Time"></param>
/// <param name="Preview"></param>
public sealed record class Reply(uint Uin, uint Sequence, long Uuid, uint Time, string Preview);
