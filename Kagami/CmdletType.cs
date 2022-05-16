namespace Kagami;

/// <summary>
/// 命令类型
/// </summary>
public enum CmdletType : byte
{
    /// <summary>
    /// 普通命令
    /// </summary>
    Normal = 0,
    /// <summary>
    /// 前缀命令
    /// </summary>
    /// <remarks>
    /// 表示这个命令只需要前缀匹配而无需空格
    /// </remarks>
    Prefix = 1,
}
