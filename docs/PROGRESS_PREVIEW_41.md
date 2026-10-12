# Progress review: Preview.41 (2026-10-07)

AP 和 AQ 核心实现及完整本地交付检查通过；最终包与源码身份由
OcctSharp/artifacts/batch-aq/delivery-result.json 冻结，[STATUS](STATUS.md) 维护实际状态。
AP–AU 六批完成 2/6，余下 AR、AS、AT、AU 四批按授权连续推进。

## 整体库存

116,272 个声明：26,504 Emitted、1,218 Manual、39,203 Blocked、49,347 Skipped，Pending=0。
AQ 精确增加 27 个 Blocked→Emitted StableId；AP 和 AQ 合计增加 93 项。
这是声明覆盖统计，不是产品完成百分比。32 个头文件仍解析失败，完整语义扫描未完成。

## 模块统计

[完整库存对账](PROGRESS_PREVIEW_41_COUNTS.json)独立核对分类及模块归属。
分母为 Emitted+Manual+Blocked，不能把分类完成当作语义扫描完成。

| 模块 | Emitted | Manual | Blocked | Skipped | 非 Skipped 覆盖率 |
|---|---:|---:|---:|---:|---:|
| Foundation | 3,524 | 10 | 7,361 | 15,568 | 32.44% |
| Geometry | 6,158 | 195 | 5,373 | 5,181 | 54.18% |
| Modeling | 2,965 | 573 | 11,492 | 8,978 | 23.54% |
| MeshData | 110 | 22 | 167 | 235 | 44.15% |
| Mesh | 171 | 2 | 317 | 572 | 35.31% |
| Documents | 1,448 | 120 | 2,370 | 3,337 | 39.82% |
| DataExchange | 7,561 | 51 | 8,064 | 8,998 | 48.56% |
| Xde | 471 | 129 | 768 | 566 | 43.86% |
| Visualization | 4,096 | 116 | 3,272 | 4,106 | 56.28% |
| IVtk | 0 | 0 | 0 | 47 | None% |
| Draw | 0 | 0 | 19 | 1,759 | 0.0% |

## 余下计划

依照[已授权 AP–AU 计划](NEXT_ROUND_AP_AU_PLAN.md)完成 AR 网格数据/诊断、
AS 文档属性/图层/注释、AT 交换头/会话筛选、AU 选择工作区/真实窗口集成。
四批逐批冻结当前基线与有用缺口。39,203 Blocked 是候选池，并非四批承诺交付量。
旧 Preview.39/40 报告保留为历史证据。

Hosted CI、签名、push、公开 NuGet 和完整下游应用验收：NOT RUN。
