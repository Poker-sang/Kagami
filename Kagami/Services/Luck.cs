namespace Kagami.Services;

public static class Luck
{
    private static readonly string[] routine =
    {
        "洗衣服", "玩游戏", "抽卡", "写代码", "奖励自己", "看新番", "补番", "宅", "摸鱼", "看书", "打5把CSGO", "多喝热水", "学数学", "看电影", "白天睡大觉",
        "出门跑步", "吃华莱士", "水群", "点外卖", "看书", "打球", "翘课", "打牌", "刷批站", "看QQ", "写论文", "肝毕设", "骑车兜风", "看涩图", "ghs", "下棋",
        "刷网课", "当舔狗", "喝阔落", "打黄油", "出嫁", "搬家", "剁手", "开空调", "泡澡", "重启电脑", "跟人干架", "出门带伞", "走路看手机", "听奥术魔刃",
        "上学", "上班", "打电动", "炼丹", "修仙", "钓鱼", "被禁言", "看比赛", "睡午觉", "翻译腔说话", "打胶", "下床", "YY群友", "看管人", "看纯爱本", "摆烂"
    };

    public struct Value
    {
        public string Draw { get; init; }
        public string[] Should { get; init; }
        public string[] Avoid { get; init; }
    }

    public static Value GetValue(long uin)
    {
        var seed = (int)(DateTime.Today.Ticks / TimeSpan.TicksPerDay) + (int)(uin % int.MaxValue);
        var random = new Random(seed);
        var draw = random.Next(100);
        var daily = new SortedSet<string>();
        while (daily.Count < 6)
            _ = daily.Add(routine[random.Next(routine.Length)]);
        return new Value
        {
            Draw = draw switch
            {
                < 15 => "凶",
                < 30 => "末吉",
                < 60 => "小吉",
                < 90 => "中吉",
                _ => "大吉",
            },
            Should = daily.ToArray()[..3],
            Avoid = daily.ToArray()[^3..]
        };
    }
}
