# ProdLogAnalyzer 项目结构说明文档

> 生成日期：2026-04-29
> 目标框架：.NET Framework 4.6.2
> 项目类型：控制台应用程序（Console Application）

---

## 目录

1. [项目整体架构概述](#1-项目整体架构概述)
2. [程序执行流程](#2-程序执行流程)
3. [文件目录组织方式](#3-文件目录组织方式)
4. [各模块功能职责](#4-各模块功能职责)
5. [核心类与关键方法说明](#5-核心类与关键方法说明)
6. [依赖关系分析](#6-依赖关系分析)
7. [配置文件说明](#7-配置文件说明)
8. [现有功能详细描述](#8-现有功能详细描述)
9. [历史遗留与注意事项](#9-历史遗留与注意事项)

---

## 1. 项目整体架构概述

ProdLogAnalyzer 是 SillyMonkey 解决方案中的一个**生产测试数据分析工具**，用于读取半导体/芯片测试过程中产生的 **STDF 格式数据文件**，对每个测试项（Test Item）进行**统计分析与正态性偏离检测**，最终输出 **HTML 可视化报告**和 **CSV 统计数据**，用于产线质量监控与异常告警。

整体采用**分层 + 管道式**架构，共分为四层：

```
┌─────────────────────────────────────────────────────────────┐
│  入口层  Program.cs  (Main)                                  │
│  · 读取 JSON 配置 · 加载/合并 STDF 数据 · 创建过滤条件         │
├─────────────────────────────────────────────────────────────┤
│  业务编排层  DataParser.cs                                   │
│  · 测试项筛选 · 规则合并 · 正态性偏离分析 · 图表生成编排       │
├─────────────────────────────────────────────────────────────┤
│  展示层  HtmlExpoter.cs / ChartGenerator.cs                  │
│  · HTML 报告生成 · ScottPlot 图表绘制                        │
├─────────────────────────────────────────────────────────────┤
│  数据层 (外部依赖项目)                                        │
│  DataContainer:  STDF 数据容器 / 过滤器 / 统计计算            │
│  FileReader:     STDF 文件解析                               │
│  SillyMonkey.Core: 汇总信息文本辅助 (SummaryHelper)          │
└─────────────────────────────────────────────────────────────┘
```

- **入口层**负责启动流程与资源调度；
- **业务编排层**是整个分析的核心控制逻辑，决定"分析哪些项、用什么规则、输出什么"；
- **展示层**只负责把分析结果渲染为 HTML 与图片；
- **数据层**以 NuGet 包或项目引用的形式提供底层数据能力，ProdLogAnalyzer 通过 `IDataAcquire` 接口与其解耦。

---

## 2. 程序执行流程

```
Main(args)
  │
  ├─ ① 无参数 → 打印 "helloworld" 后退出
  │
  ├─ ② 读取 args[0] 指定的 JSON 配置文件
  │      JsonConvert.DeserializeObject<ProdLogConfiguration>(json)
  │
  ├─ ③ 遍历 config.DataFiles 逐个加载 STDF 文件
  │      StdDB.CreateSubContainer(path)          // 创建数据容器
  │      StdReader.ExtractStdf()                 // 解析 STDF 文件
  │      da.CreateFilter()                       // 创建过滤器
  │      da.UpdateFilter(filterId, df.Filter)    // 应用外部配置的 FilterSetup
  │      记录 SubData(filePath, filterId, filterIndex)
  │
  ├─ ④ 合并所有子数据容器
  │      StdDB.MergeSubData(config.Name, subDataList)
  │      随后移除单个文件容器 (StdDB.RemoveFile)
  │
  ├─ ⑤ 创建两个过滤器
  │      filterId_raw  = 原始数据过滤器（不过滤）
  │      filterId_pass = Pass-Only 过滤器（屏蔽所有非 PassBin 的 HardBin）
  │
  └─ ⑥ 进入分析管线
         DataParser.ParseDataFile(config, dataAcquire, filterId_raw, filterId_pass)
```

### ParseDataFile 内部流程

```
ParseDataFile(config, da, filter_raw, filter_pass)
  │
  ├─ 读取 EngMode / ReportExport 开关
  ├─ 用 LotInfoRegex 从首个数据文件路径提取 Lot 信息
  ├─ 创建输出目录: <OutputFolder>/<Name>_<Lot>_<yyyyMMdd_HHmmss>/
  ├─ GenerateDataStatisticReport()
  │     · 输出 HardBin 汇总到 CSV
  │     · EngMode 下用 SummaryHelper 输出 Basic/Qty/SoftBin/HardBin 概览到 HTML
  ├─ 遍历 da.GetTestIDs() 每个测试项:
  │     · 判定 ItemAnalysePriority (ForceAnalyse / Ignore / General)
  │     · 合并 GeneralRule + 特定规则 → ItemRuleConfig
  │     · 组装 DeviationAnalysisParams (KDE / MAD 阈值参数)
  │     · AnalyzeAndGenerateReport(...)
  │           - 计算 raw / pass 两组 ItemStatistic
  │           - dataAcquire.GetFilteredNormalityDeviationResult()  ← 核心偏离分析
  │           - CalcSiteGap() 计算各 Site 中位偏移
  │           - checkDataAbnormal() 按规则判定 Pass/Fail
  │           - 输出一行 CSV 记录
  │           - 失败或 ForceAnalyse 时生成 4 张图表写入 HTML
  ├─ exporter.SaveReport() 保存 HTML
  └─ 写 CSV: <报告名>_Statistic.csv
```

---

## 3. 文件目录组织方式

### 3.1 本项目文件结构

```
ProdLogAnalyzer/
├── Program.cs                 # 程序入口
├── ProdLogConfiguration.cs    # 配置模型（反序列化目标）
├── DataParser.cs              # 核心业务编排/分析管线
├── HtmlExpoter.cs             # HTML 报告导出器
├── ChartGenerator.cs          # ScottPlot 图表生成器
├── StatisticsAnalyzer.cs      # 【历史遗留】统计计算模块（当前未被调用）
├── ProdLogAnalyzer.csproj     # 项目文件
├── App.config                 # 运行时程序集绑定重定向
├── packages.config            # NuGet 包清单
├── FodyWeavers.xml            # Costura 单文件合并配置
├── FodyWeavers.xsd
└── Properties/
    └── AssemblyInfo.cs        # 程序集元数据
```

### 3.2 输出目录结构（运行时生成）

```
<OutputFolder>/
└── <Name>_<Lot>_<时间戳>/
    ├── <报告名>.html               # 主 HTML 报告
    ├── <报告名>_Statistic.csv      # 各测试项统计 CSV
    └── images/                     # HTML 内嵌图表 (chart_<TestId>_<guid8>.png)
```

### 3.3 解决方案级相关目录

```
repo/
├── ProdLogAnalyzer/          # 本项目
├── DataContainer/            # 数据容器层（项目引用）
├── FileReader/               # STDF 文件解析层（项目引用）
├── SillyMonkey.Core/         # 核心工具层（项目引用）
└── packages/                 # NuGet 包缓存目录
```

---

## 4. 各模块功能职责

| 文件/模块 | 职责 | 备注 |
|---|---|---|
| `Program.cs` | 程序入口；读取配置；加载并合并 STDF 数据；创建 raw/pass 过滤器；调用分析管线 | 数据准备阶段 |
| `ProdLogConfiguration.cs` | 定义 JSON 配置的强类型模型（4 个类） | 反序列化目标 |
| `DataParser.cs` | 核心编排：测试项筛选、规则合并、统计/偏离分析、CSV 记录、图表与 HTML 组装 | 系统"大脑" |
| `HtmlExpoter.cs` | 生成 HTML 报告；图片落盘；CSV 描述转表格；HTML 转义 | 展示层 |
| `ChartGenerator.cs` | 基于 ScottPlot 生成趋势图、直方图、箱型图等 | 展示层 |
| `StatisticsAnalyzer.cs` | 基础统计计算（均值/标准差/Cpk/IQR 离群点等） | **遗留代码，当前无调用方** |
| `DataContainer` (外部) | STDF 数据内存容器、过滤器、`ItemStatistic`/`NormalityDeviationResult` 计算 | 数据层 |
| `FileReader` (外部) | STDF 二进制文件解析，注入数据容器 | 数据层 |
| `SillyMonkey.Core` (外部) | `SummaryHelper`：把汇总信息格式化为文本 | 工具层 |

---

## 5. 核心类与关键方法说明

### 5.1 `Program`（Program.cs）

| 成员 | 说明 |
|---|---|
| `static void Main(string[] args)` | 入口。无参数时输出 "helloworld" 退出；有参数时 `args[0]` 为配置 JSON 路径 |

关键步骤：
- 使用 **Newtonsoft.Json** 反序列化配置（兼容性好、支持注释字段忽略）。
- 每个数据文件创建独立 `SubContainer`，解析 STDF 后通过 `MergeSubData` 合并。
- `filterId_pass` 通过 `new FilterSetup("PassOnly") { MaskHardBins = allBins }` 屏蔽所有非 Pass 的 HardBin。

> ⚠️ 注意：源码中存在大量被注释掉的并行加载（`Task.Run`）代码，目前为**串行加载**。

### 5.2 `DataParser`（DataParser.cs，static class）

| 成员 | 说明 |
|---|---|
| `enum ItemAnalysePriority` | `General`（普通）、`Ignore`（忽略）、`ForceAnalyse`（强制分析） |
| `ParseDataFile(ProdLogConfiguration, IDataAcquire, int, int)` | 分析主入口 |
| `GenerateDataStatisticReport(HtmlExpoter, StringBuilder)` | 输出 HardBin 汇总与 EngMode 概览 |
| `checkDataAbnormal(ItemInfo, NormalityDeviationResult, ItemStatistic, ItemStatistic, ItemRuleConfig)` | 按规则判定测试项是否异常 |
| `AnalyzeAndGenerateReport(...)` | 单测试项分析：统计、偏离检测、CSV、图表 |
| `CalcSiteGap(...)` | 计算各 Site 中位数相对整体中位数的最大偏移比例 |
| `GenerateCharts(string, NormalityDeviationResult)` | 生成 4 张图表（对比趋势图 / Raw 直方图 / 分 Site 箱型图 / Pass 直方图） |

关键算法参数常量：`ConsistencyFactor = 1.4826f`（MAD → σ 换算系数）。

**规则判定优先级逻辑（DataParser 内）：**
1. `TargetItemsAndRuleByTestId` / `TargetItemsAndRuleByTestTextRegex` 命中 → `ForceAnalyse`；
2. `IgnoredItemsByTestId` / `IgnoredItemsByTestTextRegex` 命中 → `Ignore`；
3. 其余 → `General`（非 EngMode 下跳过 General 项）。

### 5.3 `HtmlExpoter`（HtmlExpoter.cs）

| 成员 | 说明 |
|---|---|
| `CreateReport(outputPath, title, subtitle)` | 初始化报告，创建 `images/` 目录 |
| `GenerateReport(title, description, status, charts)` | 添加一个报告区块（含图表落盘） |
| `GenerateReport(string summary)` | 添加纯文本 Summary 区块（不换行） |
| `SaveReport()` | 完成 HTML 并写入文件 |
| `RenderDescriptionAsTableIfCsv(...)` | 把两行 CSV 描述渲染为表格（RFC4180 引号解析） |
| `EscapeHtml(...)` | HTML 特殊字符转义 |

`TestStatus` 枚举：`Pass` / `Warning`。

### 5.4 `ChartGenerator`（ChartGenerator.cs）

| 方法 | 说明 |
|---|---|
| `GenerateTrendChart(data, xs, boxpara, title, min, max)` | 趋势线 + 箱型覆盖（基本图，当前主流程未直接使用） |
| `GenerateHistogram(data, boxpara, loLimit, hiLimit, title, min, max)` | 100 bins 直方图；超出上下限的 bin 标红；叠加限值线与箱型矩形 |
| `GenerateComparisonTrendChart(raw..., pass..., boxpara, title, min, max)` | 原始(蓝) vs Pass(橙) 双序列趋势图 |
| `GenerateBySiteBoxPlot(boxparas, loLimit, hiLimit, title, min, max)` | 按 Site 绘制多个箱型图 |

辅助类：
- `BitMap`：图表包装类（`Image` + `TestId` + `Description` + `FileName`）。
- `BoxPlotPara`：箱型参数（须形最小/最大、箱体四分位、中位、宽度）。

### 5.5 配置模型（ProdLogConfiguration.cs）

| 类 | 用途 |
|---|---|
| `ProdLogConfiguration` | 顶层配置：名称、EngMode、ReportExport、数据文件、规则等 |
| `DataFileConfig` | 单个数据文件：路径 + 可选 `FilterSetup` |
| `ItemRuleConfig` | 测试项规则：良率/Sigma/Cpk/均值/偏度/峰度/密度峰/离群点阈值 + KDE/MAD 参数 |
| `BinRuleConfig` | Bin 上下限规则（当前定义但主流程未使用） |

> `ItemRuleConfig` 提供深拷贝构造函数 `ItemRuleConfig(ItemRuleConfig)`，用于在 GeneralRule 基础上叠加特定规则，避免污染配置对象。

### 5.6 依赖项目的关键类型（数据层）

| 类型 | 所在项目 | 说明 |
|---|---|---|
| `IDataAcquire` | DataContainer | 数据访问接口（过滤器、统计、偏离分析） |
| `StdDB` | DataContainer | 静态数据容器注册表（内存级 DB） |
| `SubData` | DataContainer | 数据文件 + 过滤器快照 |
| `FilterSetup` | DataContainer | 过滤条件（屏蔽 Site/Bin/Chip/坐标/测试项） |
| `ItemInfo` | DataContainer | 测试项元信息（上下限、单位、缩放） |
| `ItemStatistic` | DataContainer | 单测试项统计（均值/σ/Cp/Cpk/良率等） |
| `PartStatistic` | DataContainer | 整批统计（Bin 计数、Site 计数等） |
| `NormalityDeviationResult` | DataContainer | 正态性偏离结果（密度峰数、离群点数、MAD_L/R） |
| `DeviationAnalysisParams` | DataContainer | KDE/MAD 分析参数 |
| `StdReader` / `StdV4Reader` | FileReader | STDF 文件读取 |
| `SummaryHelper` | SillyMonkey.Core | 汇总信息文本生成 |

---

## 6. 依赖关系分析

### 6.1 项目引用

| 引用项目 | 用途 |
|---|---|
| `DataContainer.csproj` | STDF 数据容器、过滤器、统计计算 |
| `FileReader.csproj` | STDF 二进制解析 |
| `SillyMonkey.Core.csproj` | `SummaryHelper` 汇总文本 |

### 6.2 NuGet 依赖（packages.config）

| 包 | 版本 | 用途 |
|---|---|---|
| `MathNet.Numerics` | 5.0.0 | 描述统计（均值/σ/偏度/峰度） |
| `ScottPlot` | 5.1.58 | 图表绘制（趋势/直方图/箱型图） |
| `SkiaSharp` | 3.119.0 | ScottPlot 渲染后端 |
| `SkiaSharp.HarfBuzz` / `HarfBuzzSharp` | 3.119.0 / 8.3.1.1 | 字体渲染 |
| `Newtonsoft.Json` | 13.0.4 | JSON 配置解析 |
| `System.Text.Json` | 9.0.9 | 部分运行时依赖 |
| `System.Drawing.Common` | 4.7.2 | `System.Drawing.Image` 位图 |
| `Costura.Fody` | 6.2.0 | 构建时把 DLL 内嵌为单文件 EXE |
| `Fody` | 6.9.3 | Costura 的 weaver 宿主 |

> 依赖传递链：ScottPlot 5.x → SkiaSharp → HarfBuzzSharp；MathNet.Numerics 用于统计计算。

### 6.3 构建配置

- 目标框架：`v4.6.2`
- 平台：`AnyCPU`（另配置了 `ARM64` 平台输出）
- `AutoGenerateBindingRedirects = true`，`App.config` 中声明了 `System.ValueTuple`、`System.Memory`、`System.Text.Json` 等程序集绑定重定向。
- 通过 **Costura.Fody** 将托管依赖嵌入程序集，输出**单文件可执行程序**（`bin\Debug\ProdLogAnalyzer.exe`）。

---

## 7. 配置文件说明

### 7.1 命令行配置 JSON

运行时通过 `args[0]` 传入 JSON 配置文件路径，例如：

```json
{
  "Name": "LOT123",
  "EngMode": false,
  "ReportExport": true,
  "LotInfoRegex": "(LOT[0-9]+)",
  "PassBin": 1,
  "DataFiles": [
    { "Path": "D:\\stdf\\lot1.stdf" },
    { "Path": "D:\\stdf\\lot2.stdf",
      "Filter": { "MaskSites": [2, 3], "MaskHardBins": [2, 3, 4] } }
  ],
  "IgnoredItemsByTestId": ["302011_0"],
  "IgnoredItemsByTestTextRegex": ["_CAL_", "BIN[0-9]+"],
  "GeneralRule": {
    "YieldLimit_Low": 0.95,
    "CpkLimit_Low": 1.33,
    "Para_MAD_Threshold_Left": 4.0,
    "Para_MAD_Threshold_Right": 4.0
  },
  "TargetItemsAndRuleByTestId": {
    "302011_0": { "SigmaLimit_High": 5.0 }
  },
  "TargetItemsAndRuleByTestTextRegex": {
    "VDD.*": { "MeanLimit_Low": 1.7, "MeanLimit_High": 1.9 }
  },
  "OutputFolder": "D:\\output"
}
```

### 7.2 配置项含义

| 字段 | 类型 | 含义 |
|---|---|---|
| `Name` | string | 批次名，用于合并容器与输出目录命名 |
| `EngMode` | bool | 工程模式：分析全部测试项并输出更详细报告 |
| `ReportExport` | bool | 是否导出 HTML 报告 |
| `LotInfoRegex` | string | 从数据文件路径提取 Lot 信息的正则 |
| `PassBin` | int | 判定 Pass 的 HardBin 编号（默认 1） |
| `DataFiles[]` | array | 待分析的数据文件（可带过滤条件） |
| `IgnoredItemsByTestId` | string[] | 按 TestID 精确忽略的测试项 |
| `IgnoredItemsByTestTextRegex` | string[] | 按 TestText 正则忽略的测试项 |
| `GeneralRule` | ItemRuleConfig | 所有测试项通用规则 |
| `TargetItemsAndRuleByTestId` | dict | 指定 TestID 的强制分析与规则 |
| `TargetItemsAndRuleByTestTextRegex` | dict | 指定 TestText 正则的强制分析与规则 |
| `OutputFolder` | string | 报告输出根目录 |

`ItemRuleConfig` 支持字段：`YieldLimit_High/Low`、`SigmaLimit_High/Low`、`CpkLimit_High/Low`、`MeanLimit_High/Low`、`SkewnessLimit_High/Low`、`ExcessKurtosisLimit_High/Low`、`ModeCountLimit`、`OutliersLimit`，以及 KDE/MAD 参数 `Para_BandwidthRatio`、`Para_KernelSampleCount`、`Para_PeakProminenceRatio`、`Para_MAD_Threshold_Left/Right/HalfLimit`。

---

## 8. 现有功能详细描述

### 8.1 数据加载与合并
- 支持多个 STDF 文件合并为单一批次（`StdDB.MergeSubData`），合并后移除单文件容器释放内存。
- 支持按文件级 `FilterSetup` 预过滤（如屏蔽坏 Site、坏 Bin）。

### 8.2 双过滤器分析（raw vs pass）
- `filterId_raw`：全量数据。
- `filterId_pass`：仅 Pass 数据（屏蔽所有非 PassBin 的 HardBin）。
- 两组统计同时计算，便于对比良率与分布差异。

### 8.3 正态性偏离检测（核心算法，由 DataContainer 提供）
`GetFilteredNormalityDeviationResult` 基于以下技术：
1. **MAD 稳健统计**：计算中位数左右两侧的 MAD（`MAD_L` / `MAD_R`），配合 `ConsistencyFactor = 1.4826` 换算 σ；
2. **KDE 核密度估计**：估计分布密度曲线并寻找**密度峰**（`ModeCount`），用于检测双峰/多峰分布；
3. **峰值显著性**：`PeakProminenceRatio` 控制峰的显著性阈值；
4. **MAD 离群点**：超出 `MadOutlierThRatio × 1.4826 × MAD` 的数据点计为离群点（`OutlierCount`）。

### 8.4 异常判定规则（checkDataAbnormal）
依次检查：上下限存在性 → 良率 → 常数数据 → σ → Cpk → 均值 → 偏度 → 峰度 → 密度峰数 → 离群点数。任一超限即判 Fail；无上下限且非常规项默认通过。

### 8.5 Site 差异监控（CalcSiteGap）
对每个 Site 计算其中位数与整体中位数的相对偏移（按规格范围归一化），输出最大偏移比例，用于发现 Site 间偏差。

### 8.6 报告输出
- **HTML 报告**：Header 区（批次信息/时间）+ 每个测试项一个区块（状态徽章 + 描述表格 + 图表网格），图片按两列自适应布局。
- **CSV 统计**：每测试项一行：`TestID,TestText,LoLimit,HiLimit,数据量,良率,平均值,标准差,CPK,偏度,峰度,密度峰,离群点[,Median,MAD_L,MAD_R,SiteGap,结果]`（EngMode 附加列）。
- **整批汇总**：HardBin/SoftBin 计数（EngMode 下输出 Basic Info/Qty/SoftBin/HardBin 概览文本）。

### 8.7 图表（共 4 张/测试项）
1. 原始 vs Pass 对比趋势图（含箱型覆盖）；
2. Raw 数据直方图（上下限红色虚线标注、超限 bin 红色）；
3. 分 Site 箱型图（多 Site 分布对比）；
4. Pass 数据直方图。

---

## 9. 历史遗留与注意事项

### 9.1 遗留代码
| 文件 | 状态 | 说明 |
|---|---|---|
| `StatisticsAnalyzer.cs` | **未使用** | 提供 `StatisticResult`/`AnomalyAnalysis`/MAD 清洗等能力，但当前 `DataParser` 已改用 `DataContainer.ItemStatistic` + `NormalityDeviationResult`，此文件暂无调用方。若后续开发新的独立分析功能可复用，否则建议清理 |
| `BinRuleConfig` | 未使用 | 已在配置模型中定义，但 `DataParser` 未读取 `SoftBinRule`/`HardBinRule` |
| `PowerPointExporter.cs` | 已删除 | 早期基于 OpenXML/ShapeCrawler 的 PPT 导出器，已从 `csproj` 中移除，`Program.cs` 中相关注释代码亦已禁用 |

### 9.2 注意事项
1. **单文件发布**：Costura.Fody 会把依赖 DLL 内嵌进 EXE，首次构建需确保 `packages` 目录完整（`EnsureNuGetPackageBuildImports` 会在缺包时报错）。
2. **大数据量内存**：STDF 数据全部加载进内存容器（`SubContainer`），超大批次可能占用较高内存。
3. **并行加载已注释**：`Program.cs` 中 `Task.Run` 并行加载代码被注释，当前为串行，若有性能需求可恢复并行并注意 `SubContainer` 线程安全。
4. **编码约定**：`DataParser.cs` / `HtmlExpoter.cs` 文件头部 `using` 存在冗余（如 `MathNet.Numerics.Statistics`、`ScottPlot.Statistics`），不影响编译但可清理。
5. **无 PPT 导出**：当前版本仅输出 HTML + CSV，不含 PPT。

---

*本文档基于 `ProdLogAnalyzer.csproj` 及源码实际内容撰写，为后续功能开发提供技术参考。*
