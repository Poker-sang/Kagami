using System.ComponentModel;

namespace Kagami.ArgTypes;

public enum MemeOption
{
    [Description("发送沙雕图")]
    Send,
    [Description("更新沙雕图")]
    Update,
    [Description("列出全部沙雕图")]
    List,
    [Description("发送整期沙雕图")]
    All
}
