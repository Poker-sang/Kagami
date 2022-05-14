using Kagami.ArgTypes;
using Kagami.Extensions;
using Konata.Core;
using Konata.Core.Events.Model;
using Konata.Core.Interfaces.Api;
using Konata.Core.Message;
using static Kagami.Services.Meme;

namespace Kagami.Commands;

internal class Meme : IKagamiCmdlet
{
    public string Command => "meme";

    public string Description => "弔图相关";

    (Type Type, string Description)[][] OverloadableArgumentList { get; } =  {
        new (Type Type, string Description)[0]{},
        new []{ (typeof(MemeCommands),"弔图指令") },
        new []{ (typeof(uint),"期数") },
        new []{
            (typeof(MemeCommands),"弔图指令"),
            (typeof(uint),"期数")
        }
    };
    public async Task<MessageBuilder> InvokeAsync(Bot bot, GroupMessageEvent group, object[] args)
    {
        // 期数的阿拉伯数字字符串
        string? issue;
        switch (args.FirstOrDefault())
        {
            case MemeCommands command:
                switch (command)
                {
                    // 列出已有期数
                    case MemeCommands.List:
                        return new(new DirectoryInfo(MemePath).GetDirectories().Aggregate("弔图已有期数：",
                            (current, directoryInfo) => current + directoryInfo.Name + ", ")[..^2]);
                    // 更新图片
                    case MemeCommands.Update:
                        {
                            _ = await bot.SendGroupMessage(group.GroupUin, new MessageBuilder("正在获取弔图..."));
                            // 图片链接索引
                            string[]? imgUrls = null;
                            // 期数的汉字数字字符串
                            string? cnIssue = null;
                            // 未指定期数，RSS获取订阅
                            if (args.Length is 1)
                                try
                                {
                                    (imgUrls, cnIssue) = await GetMemeImageSourcesRssAsync();
                                    issue = cnIssue.CnToInt().ToString();
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine(e);
                                    return new(e.Message);
                                }
                            // 指定期数
                            // var result = Regex.Match(content[0], @"\b[0-9]+\b");
                            else
                                issue = (string)args[1];
                            // 下载图片
                            var directory = new DirectoryInfo(MemePath + issue);
                            try
                            {
                                // 如果已经有文件夹
                                if (directory.Exists)
                                {
                                    // 只有索引
                                    if (directory.GetFiles() is { Length: 2 } files &&
                                        files[1] is { Name: Indexer } txtFile)
                                        imgUrls ??= await File.ReadAllLinesAsync(txtFile.FullName);
                                    // 索引和图片都有
                                    else
                                        return new($"{issue}期弔图已存在！");
                                }
                                // 没有文件夹
                                else
                                {
                                    // may throw FormatException
                                    var newInt = int.Parse(issue);
                                    // 指定期数时，需要阿拉伯数字转汉字
                                    cnIssue ??= newInt.IntToCn();
                                    // 指定期数时，需要下载图片链接索引
                                    // may throw EdgeDriverBusyException, NotFoundException
                                    imgUrls ??= await GetMemeImageSourcesAsync(cnIssue);
                                    directory.Create();
                                    // 记录索引和指针
                                    await File.WriteAllTextAsync(Path.Combine(directory.FullName, Pointer), 0.ToString());
                                    await File.WriteAllLinesAsync(Path.Combine(directory.FullName, Indexer), imgUrls);

                                    // 获取图片
                                    for (var i = 0; i < imgUrls.Length; ++i)
                                        await File.WriteAllBytesAsync(Path.Combine(directory.FullName, (i + 2).ToString()),
                                            await imgUrls[i].DownloadBytesAsync());

                                    // 若已有最新的则不写入总索引
                                    if (int.Parse(await File.ReadAllTextAsync(NewPath)) < newInt)
                                        await File.WriteAllTextAsync(NewPath, issue);
                                }
                            }
                            catch (FormatException e)
                            {
                                Console.WriteLine(e);
                                return new(await StringResources.ArgumentErrorMessage.RandomGetAsync());
                            }
                            catch (FileNotFoundException e)
                            {
                                Console.WriteLine(e);
                                return new(e.Message);
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e);
                                return new("弔图更新失败！你可以重新尝试");
                            }
                            return new($"弔图已更新！第{issue}期");
                        }
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            // 发送图片
            // 未指定期数
            case null:
                try
                {
                    if (!File.Exists(NewPath))
                        return new("仓库里还没有弔图，先更新吧x");
                    issue = await File.ReadAllTextAsync(NewPath);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    return new(e.Message);
                }
                break;
            // 指定期数
            case uint num:
                // var result = Regex.Match(content[0], @"\b[0-9]+\b");
                issue = num.ToString();
                break;
            default:
                throw new ArgumentOutOfRangeException();

        }

        try
        {
            var directory = new DirectoryInfo(MemePath + issue);

            // 指定期数不存在
            if (!directory.Exists)
                return new($"第{issue}期不存在");

            var pointer = uint.Parse(await File.ReadAllTextAsync(Path.Combine(directory.FullName, Pointer)));
            var files = directory.GetFiles();
            if (pointer >= files.Length - 2)
                pointer = 0;

            var image = await File.ReadAllBytesAsync(Path.Combine(directory.FullName,
                (pointer + 2).ToString()));

            ++pointer;
            await File.WriteAllTextAsync(Path.Combine(directory.FullName, Pointer), pointer.ToString());

            var message = new MessageBuilder();
            message.Text($"{issue} {pointer}/{files.Length - 2}");
            message.Image(image);
            return message;
        }
        catch (FormatException e)
        {
            Console.WriteLine(e);
            return new(await StringResources.ArgumentErrorMessage.RandomGetAsync());
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return new(e.Message);
        }
    }
}