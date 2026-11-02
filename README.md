# Tartarus Solver（中文版）

> 本仓库是 [glcoder/TartarusSolver](https://github.com/glcoder/TartarusSolver) 的中文化分支，界面和文档译为简体中文。
> 原项目作者为 Artem Gurevich，以 MIT 协议发布。功能已同步至 [v0.7.36](https://github.com/glcoder/TartarusSolver/commit/407e2800bf118c11248d28134b68caff25f4b932)，保留中文界面和本分支的控制窗口布局。

为《火炬之光：无限》（第 16 赛季）的沙盘玩法**塔罗斯监狱**提供走法提示，以叠加层的形式绘制在游戏窗口之上。

程序读取屏幕画面和游戏的日志文件，推算出当前局面，按游戏规则模拟数千次，然后框出能让本局结束时绮彩囚犯最多
（其次是暴乱宝箱最丰厚）的那一步。

> **它不会做的事：** 不读写游戏内存，不向游戏注入任何东西，也不在游戏里按键或点击。它只捕获游戏窗口
> （Windows 屏幕捕获 API），并读取游戏写到磁盘上的日志文件。每一步都由你自己操作。

## 功能

- **走法提示：** 最佳走法用绿色粗框标出，框上显示本局预计结束时的宝箱数（绮彩和至臻）；当最佳走法是「吃」或「寻猎」卡时，
  还会用虚线框标出下一次点击的目标格。每个决策都有提示：目的地、请进行决策（吃、换、寻猎、结束、煽动、赏赐）、「吃」的目标
  选择和连吃、「寻猎」的目标选择、暗渠鼠道、刑具选择（含重新随机）以及典狱长的珍藏。
- **游戏规则：** 全部 31 种刑具、17 种狱中事变、目标、各局主题（滥权、奢靡、铁律、囚徒困斗）、鼠巢、象牙和猎物标记、划痕。
- **求解器：** 确定化蒙特卡洛搜索（门后隐藏的内容在每次模拟中重新抽取，不偷看）：模拟中的走法由一个小型策略网络给出，
  它用搜索自身的选择训练而成；模拟的终点由一个小型价值网络估值。
- **屏幕识别：** 用卷积网络识别格子（代号、稀有度、标记），用小型网络识别刑具槽和绮彩槽，卡牌和选择界面按图片识别，
  文字用 OCR（PaddleOCR，ONNX）识别。支持游戏的所有语言：内置英文、俄文和中文 OCR，其他语言按需下载；首次连接
  游戏时会询问语言，并标出从游戏设置中检测到的语言，也可以随时在控制窗口中选择。
- **游戏日志：** 从游戏写出的日志文件读取开局、天数、吃、绮彩、狱中事变和各项选择，补全屏幕上看不到的信息
  （红色的「吃」界面、弹窗）。
- **统计：** 根据游戏日志统计已玩局数和结束时的绮彩囚犯数（总数和每局平均），重启后保留，直到手动重置。
- **错误报告：** 按一个快捷键即可把带叠加层的截图、最近几帧、程序日志和本局的游戏日志发给开发者（构建时配置了
  Sentry DSN 才会发送，否则报告保存在本地）。窗口卡住 5 秒会自动报告，并附上程序当时的状态（自身各线程的调用栈）。

## 系统要求

- Windows 10（2004）或 11，x64。
- [.NET 9 桌面运行时](https://dotnet.microsoft.com/download/dotnet/9.0)（使用 `-full` 发布包则不需要）。
- 《火炬之光：无限》需以窗口或无边框模式运行：叠加层无法绘制在独占全屏之上。

## 安装与使用

1. 下载发布版 zip，解压到任意位置，运行 `TartarusSolver.exe`。
2. 启动游戏并进入塔罗斯监狱。程序会自动找到游戏窗口和日志文件（也可以在控制窗口中手动选择）。
3. 开始游戏。叠加层会针对屏幕上的内容显示提示。

新版本：开启「检查更新」（默认开启）时，程序在启动时和之后每 6 小时向 GitHub 查询最新发布版；有新版本时，
点击「更新到 …」即可下载、校验并重启到新版本。除此之外不会发送任何数据。
| 快捷键 | |
|---|---|
| Ctrl+Shift+S | 立即扫描 |
| Ctrl+Shift+R | 开关自动扫描（默认每 0.5 秒一次） |
| Ctrl+Shift+H | 开关叠加层 |
| Ctrl+Shift+B | 报告错误 |
| Ctrl+Shift+E / Ctrl+Shift+D | 标注编辑 / 保存当前帧（用于制作训练集；需 `--debug`） |

游戏或本程序在前台时快捷键均有效。控制窗口中可以设置游戏窗口、自动扫描及其间隔（0 到 1 秒；0 表示上一帧识别和提示
完成后立即处理下一帧）、游戏语言、提示强度、游戏日志和错误报告。`TartarusSolver.exe --debug` 会打开完整窗口：
叠加层视图（区域、OCR 文字、全部识别结果）、标注、数据集收集和程序日志。

设置保存在 `%LOCALAPPDATA%\TartarusSolver\settings.json`，其中包括：

- `CacheWorth`：每个剩余囚犯对应的暴乱宝箱折合多少个绮彩，按稀有度（普通 … 至臻）分别设置；求解器据此权衡
  稳拿的宝箱和用它换一次绮彩的机会。
- `DetectGameLanguage`：是否从游戏设置中自动读取语言（「自动检测」开关）；在语言列表中手动选择会关闭自动检测。
- `GameLanguage`：手动选择的游戏语言（`en`、`ru`、`zh` 等）；关闭自动检测后使用。为空时，在首次连接游戏时询问。
- `HintPlayouts`、`HintSeconds`：求解器每步的计算量（也可用控制窗口中的滑块调节）。
- `CheckUpdates`：是否向 GitHub 查询新版本（窗口中的复选框）。
- `Hotkeys`、`GameLogPath`、`ReportDsn`。

## 构建

```bash
dotnet build TartarusSolver.sln -c Release -p:Platform=x64
```

OCR 模型（PaddleOCR，Apache-2.0）在首次构建时下载到 `models/ocr`，并做 SHA-256 校验。
生成的程序位于 `src/Tartarus.App/bin/x64/Release/net9.0-windows10.0.22621.0/win-x64/TartarusSolver.exe`。

```bash
dotnet test tests/Tartarus.Rules.Tests -c Release
```

打包给他人使用的发布版 zip（只含程序需要的文件，附简短说明；`--full` 会打包 .NET）：

```bash
python tools/package.py
```

构建时提供 DSN，错误报告就会发送到 Sentry：设置环境变量 `TARTARUS_SENTRY_DSN`（或传入 `-p:SentryDsn=…`，或在设置中
填写 `ReportDsn`）。没有 DSN 时，报告保存在 `%LOCALAPPDATA%\TartarusSolver\reports`。

## 项目结构

| | |
|---|---|
| `src/Tartarus.Rules` | 游戏规则：局面状态、合法走法、所有刑具和事变、计分 |
| `src/Tartarus.Solver` | 蒙特卡洛搜索、启发式策略、价值网络和策略网络 |
| `src/Tartarus.Vision` | 屏幕布局、OCR、格子和槽位网络、卡牌和选择图片、游戏日志、数据集 |
| `src/Tartarus.Capture` | 窗口捕获（Windows.Graphics.Capture） |
| `src/Tartarus.App` | WPF 叠加层和控制窗口、提示、错误报告 |
| `tests/Tartarus.Rules.Tests` | 规则、求解器和屏幕文字测试 |
| `tools/Tartarus.ScanCli` | `tartarus-scan`：扫描帧、导出训练裁图、标注检查页面、日志分析 |
| `tools/Tartarus.SimCli` | `tartarus-sim`：模拟对局、测量强度、为网络生成自我对弈数据 |
| `tools/train` | 用 PyTorch 训练各网络（`requirements.txt`） |
| `models` | 训练好的网络，以及程序识别用的图片和文字 |

## 训练

各网络用游戏时捕获的帧训练：程序把帧保存到 `dataset/`（自动收集，或按 Ctrl+Shift+D），标注在编辑模式或
`tartarus-scan review` 页面中人工核对。

```bash
tartarus-scan export dataset                      # 裁图和标注导出到 dataset/train
python tools/train/cellnet.py dataset/train       # 格子和典狱长箭头 -> models/cells
python tools/train/slotnet.py dataset/train       # 刑具槽和绮彩槽 -> models/slots
tartarus-sim selfplay --runs 20000 --out values-1.bin
python tools/train/valuenet.py values-*.bin       # 价值网络 -> models/solver/value.bin
tartarus-sim policydata --runs 20000 --policy vn:64:3 --out actions-1.bin
python tools/train/policynet.py actions-*.bin     # 模拟用的策略网络 -> models/solver/policy.bin
```

可选：`policydata --values-out values.bin` 会把同一批对局的每日局面也写出来，供价值网络使用；
`policynet.py --instruments --out models/solver/picks.bin` 训练一个用于模拟中刑具选择的网络，该文件存在时才会启用。
`tartarus-sim compare` 评估一项改动（4000 局配对对局），`tartarus-sim levels` 重新测量 `models/solver/levels.txt`，
`tartarus-sim calib --table models/solver/chests.txt` 重新拟合提示中的预计宝箱数与对局实际结果的对应关系，
`tartarus-sim objcheck` 比较搜索在完成目标之前和选择目标时的估值。模拟器默认以 50% 的概率给一局提供重新随机
（宠物 Twin Inquisitors），可用 `--reroll-percent` 修改。

## 免责声明

本工具为玩家自制，与《火炬之光：无限》的开发商或发行商无关，也未获其认可。`models/` 中游戏的名称、文字和图片
（图标、卡牌模板、`text/tartarus_text.json`）归其所有者所有，收录它们只是为了让程序能识别游戏画面。

## 致谢

- **[CoffeeBns](https://www.twitch.tv/coffeebns)**：感谢其制作的练习模拟器
  [Tartarus Simulator](https://blahdot3h.github.io/tartarus-simulator/)，本项目由它发展而来（它的规则、实时统计和
  截图识别是规则引擎和第一版扫描器的起点），也感谢其推动了本项目的诞生。
- **[tli-warden](https://github.com/arcasti/tli-warden)**：它从游戏客户端提取的数据（稀有度权重、刑具、狱中事变、
  目标和主题）帮助确定了规则中的数值。
- **[TLI Overlay Plus](https://github.com/Giboork/tli-overlay-plus)**：它的顾问功能所用的规则数值是核对本项目数值的
  第二参考（其中一条，即某些早晨少一张目的地卡，与本项目一致）。

本项目未使用以上项目的任何代码。

## 许可证

[MIT](LICENSE) © 2026 Artem Gurevich。原仓库：[glcoder/TartarusSolver](https://github.com/glcoder/TartarusSolver)。
第三方组件保留各自的许可证：ONNX Runtime（MIT）、PaddleOCR 模型（Apache-2.0）、Sentry .NET SDK（MIT）、
ClrMD（MIT）、Vortice.Windows（MIT）、StbImageSharp（公有领域）。
