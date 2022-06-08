namespace Kagami.Services;

public static class Luck
{
    private static readonly string[] routine =
    {
        "洗衣服", "问问题", "抽卡", "写代码", "奖励自己", "看新番", "补番", "宅", "摸鱼", "看书", "打5把CSGO", "多喝热水", "学数学", "看电影", "白天睡大觉",
        "出门跑步", "吃华莱士", "水群", "点外卖", "看书", "打球", "翘课", "打牌", "刷批站", "看QQ", "写论文", "肝毕设", "骑车兜风", "看涩图", "ghs", "下棋",
        "刷网课", "当舔狗", "喝阔落", "打黄油", "出嫁", "搬家", "剁手", "开空调", "泡澡", "重启电脑", "跟人干架", "出门带伞", "走路看手机", "听奥术魔刃",
        "上学", "上班", "打电动", "炼丹", "修仙", "钓鱼", "被禁言", "看比赛", "睡午觉", "翻译腔说话", "打胶", "下床", "YY群友", "看管人", "看纯爱本", "摆烂",
        "卖萌", "表白", "女装", "犯傻", "旅游", "吃冰棍", "开后宫", "吃辣条", "看NTR本", "画本子", "复习", "嗦粉", "调戏", "买菜", "讨价还价", "要求加薪",
        "上课发言", "堇业", "逛街", "挂机", "K歌", "试新衣服", "穿泳装", "去游泳", "吃瓜", "啖荔枝", "找人对线", "约会", "散步", "写日记", "打电话", "给自己礼物",
        "做饭", "洗碗", "整理房间", "独处", "抱怨", "喝鸡汤", "吹牛", "逗猫猫", "理发", "求签", "荡秋千", "复读", "Debug"
    };

    public struct Value
    {
        public string Draw { get; init; }
        public string[] Should { get; init; }
        public string[] Avoid { get; init; }
    }

    public static Value GetValue(long uin)
    {
        var seed = (int)(DateTime.Today.Ticks / TimeSpan.TicksPerDay + (uin % int.MaxValue));
        var random = new Random(seed);
        var draw = random.Next(100) switch
        {
            < 15 => "凶",
            < 30 => "末吉",
            < 60 => "小吉",
            < 90 => "中吉",
            _ => "大吉",
        };
        var daily = new SortedSet<string>();
        while (daily.Count < 6)
            _ = daily.Add(routine[random.Next(routine.Length)]);
        return new Value
        {
            Draw = draw,
            Should = daily.ToArray()[..3],
            Avoid = daily.ToArray()[^3..]
        };
    }
}
