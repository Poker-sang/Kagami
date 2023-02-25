using Kagami.UsedTypes;
using Konata.Core.Message;
using System.ComponentModel;
using Wolfram.NETLink;

namespace Kagami.Commands;

public class Mathematica
{
    [Obsolete("有安全问题")]
    [Cmdlet(nameof(Mma), CmdletType = CmdletType.Prefix), Description("用Mathematica计算")]
    public static async ValueTask<MessageBuilder> Mma([Description("Mathematica语句")] string cmd)
    {
        await Task.Yield();
        var ml = MathLinkFactory.CreateKernelLink();
        try
        {
            ml.WaitAndDiscardAnswer();
            return new(ml.EvaluateToInputForm(cmd, 0));
        }
        finally
        {
            ml.Close();
        }
    }
}
