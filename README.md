# Kagami

用 [Konata.Core](https://github.com/KonataDev/Konata.Core) 框架搭建的QQ机器人

![C#](https://img.shields.io/badge/C%23-latest-green)
[License](https://img.shields.io/static/v1?label=LICENSE&message=GNU%20GPLv3&color=lightrey)
<img width="20" src="https://github.com/KonataDev/Konata.Core/raw/main/Resources/konata_icon_512_round64.png">
<img width="20" src="https://user-images.githubusercontent.com/17957399/157422004-2a367049-3243-4206-90f4-ecb3f033c5ab.png">
<img width="20" src="https://user-images.githubusercontent.com/17957399/155513020-dd912c37-a86f-4d67-b707-566418cbc152.png">
<img width="20" src="https://user-images.githubusercontent.com/17957399/157422071-0faf24e0-46c6-4617-8dc0-ba6eab193237.png">

## 命令

| 命令 | 功能 |
| - | - |
| help | 输出帮助信息 |
| greeting | 打招呼 |
| status | 输出状态如内核状态、内存使用等 |
| recall | 撤回机器人的上一条消息 |
| repeat -message | 复读一条消息 |
| meme [-command] [-issue] | 获取梗图（默认最新期） |
| mute -at [-minute] | 禁言某成员（默认10分钟） |
| title -at -title | 为某成员设置头衔 |
| member -at | 查看成员信息 |
| bv -code | 通过 BV 获取视频信息 |
| github -organization -repository | 获取 GitHub 图片 |

## 触发器

| 名称 | 触发 | 功能 |
| - | - | - |
| repeat | 3条连续相同的文字消息 | 复读 |
| recall | 回复机器人某条信息 | 撤回 |

## 希望加的功能

- [ ] 识别色图并撤回

- [ ] 人工智能回复

## 证书

Licensed in GNU GPLv3 with ❤.
