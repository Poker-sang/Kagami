using Kagami.Attributes;

namespace Kagami.ArgTypes;

public enum MemeCommands
{
    [EnumHelp("发送沙雕图")]
    None,
    [EnumHelp("更新沙雕图")]
    Update,
    [EnumHelp("列出全部沙雕图")]
    List
}