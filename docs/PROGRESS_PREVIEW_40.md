# Progress review: Preview.40 (2026-10-07)

AP 的核心实现及完整本地交付检查通过；最终冻结身份由
OcctSharp/artifacts/batch-ap/delivery-result.json 维护。[STATUS](STATUS.md) 是当前状态入口。
本轮 AP–AU 六批：AP 完成后 1/6，AQ–AU 还剩五批，按授权连续推进。
AF–AK 和 AL–AO 的十个历史批次已经交付。

## 整体库存

116,272 个声明：26,477 Emitted、1,218 Manual、39,230 Blocked、49,347 Skipped，Pending=0。
AP 增加 66 个准确的 Blocked→Emitted StableId，没有删除旧生成文件或 public API。
已绑定 27,695 项，占全库存 23.8191%，排除 Skipped 后为 41.3821%。
这些是声明覆盖率，不能解释为产品完成百分比。
32 个头文件仍解析失败，未发现的声明无法计入分母；完整语义扫描尚未完成。

## 模块统计

下表由[完整库存对账](PROGRESS_PREVIEW_40_COUNTS.json)生成，覆盖率分母为 Emitted+Manual+Blocked。
这份库存统计独立于交付冻结，仅核对分类和模块归属；不把分类完成当作语义扫描完成。

| 模块 | Emitted | Manual | Blocked | Skipped | 非 Skipped 覆盖率 |
|---|---:|---:|---:|---:|---:|
| Foundation | 3,524 | 10 | 7,361 | 15,568 | 32.44% |
| Geometry | 6,131 | 195 | 5,400 | 5,181 | 53.95% |
| Modeling | 2,965 | 573 | 11,492 | 8,978 | 23.54% |
| MeshData | 110 | 22 | 167 | 235 | 44.15% |
| Mesh | 171 | 2 | 317 | 572 | 35.31% |
| Documents | 1,448 | 120 | 2,370 | 3,337 | 39.82% |
| DataExchange | 7,561 | 51 | 8,064 | 8,998 | 48.56% |
| Xde | 471 | 129 | 768 | 566 | 43.86% |
| Visualization | 4,096 | 116 | 3,272 | 4,106 | 56.28% |
| IVtk | 0 | 0 | 0 | 47 | None% |
| Draw | 0 | 0 | 19 | 1,759 | 0.0% |

## 剩余计划

依照[已授权 AP–AU 计划](NEXT_ROUND_AP_AU_PLAN.md)继续 AQ 几何扫描线工作区、
AR 网格数据及网格化诊断、AS 文档属性/图层/注释、AT 交换头与会话筛选、
AU 选择工作区及真实窗口集成。各批启动时冻结候选和准入；不能把 39,230 Blocked
等同于这五批承诺交付量。全部六批交付后再按实测缺口规划。

公开 NuGet、签名、push、Hosted CI 和完整下游应用验收仍 NOT RUN。
