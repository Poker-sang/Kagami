using System.Text;

namespace Kagami.Extensions;

internal static class CnIntConvertExtensions
{
    private const string CnNumber = "零一二三四五六七八九十百千万";

    private static readonly List<string> cnUnit = new() { "", "十", "百", "千", "万", "十万", "百万", "千万", "亿" };

    /// <summary>
    /// 阿拉伯数字转中文数字
    /// </summary>
    /// <param name="integer"></param>
    /// <returns></returns>
    public static string IntToCn(this int integer)
    {
        if (integer is 0)
            return "零";
        var cnNumber = new StringBuilder();
        for (var i = 0; integer is not 0; ++i)
        {
            var temp = integer % 10;
            if (temp is not 0)
            {
                _ = cnNumber.Insert(0, cnUnit[i]);
                _ = cnNumber.Insert(0, CnNumber[temp]);
            }
            else if (cnNumber.Length is not 0 && cnNumber[0] is not '零')
                _ = cnNumber.Insert(0, "零");
            integer /= 10;
        }

        return cnNumber.ToString();
    }

    public static int CnToInt(this string cnNumber)
    {
        var integer = 0;
        cnNumber = cnNumber.Replace("零", "");
        // 从亿循环到十位
        for (var i = cnUnit.Count - 1; i > 0; --i)
            if (cnNumber.Split(cnUnit[i]) is { Length: 2 } splitCnNumber)
            {
                integer += (int)Math.Pow(10, i) * CnNumber.IndexOf(splitCnNumber[0], StringComparison.Ordinal);
                cnNumber = splitCnNumber[1];
            }

        integer += CnNumber.IndexOf(cnNumber, StringComparison.Ordinal);
        return integer;
    }
}

