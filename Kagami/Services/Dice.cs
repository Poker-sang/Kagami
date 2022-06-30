namespace Kagami.Services;

/// <summary>
/// 从random.org获取随机数的骰子
/// </summary>
public static class Dice
{
    /// <summary>
    /// 投掷骰子
    /// </summary>
    /// <param name="num">投掷次数</param>
    /// <param name="min">最小点数</param>
    /// <param name="max">最大点数</param>
    /// <returns>以换行符分割的字符串</returns>
    public static Task<string> RollAsync(int num, int min, int max)
        => $"https://www.random.org/integers/?num={num}&min={min}&max={max}&col=1&base=10&format=plain&rnd=new"
        .DownloadStringAsync();

    /// <summary>
    /// 投掷一次骰子
    /// </summary>
    /// <param name="min">最小点数</param>
    /// <param name="max">最大点数</param>
    /// <returns>以换行符分割的字符串</returns>
    public static Task<string> RollAsync(int min, int max) => RollAsync(1, min, max);

    /// <summary>
    /// 投掷一次骰子
    /// </summary>
    /// <param name="max">最大点数</param>
    /// <returns>以换行符分割的字符串</returns>
    public static Task<string> RollAsync(int max) => RollAsync(1, 0, max);
}
