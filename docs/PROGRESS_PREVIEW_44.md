# Progress review: Preview.44 (2026-10-08)

AP–AS 已交付；AT 的实现和完整本地检查通过，只有
OcctSharp/artifacts/batch-at/delivery-result.json 为 PASS 才计为交付。
AT 冻结后本轮完成 5/6，AU 继续按授权完成选择工作区及真实窗口集成。

116,272 声明：26,669 Emitted、1,227 Manual、39,029 Blocked、49,347 Skipped；Pending=0。
AT 新增 67 generated StableId（63 主准入、4 前置），30 已有声明复用，119 主候选暂缓。
本轮五批累计 258 generated、9 private Manual。声明覆盖不等同产品完成。
32 个 parser exclusions 未改变，完整语义扫描仍有排除。

| 模块 | Emitted | Manual | Blocked | Skipped | 非 Skipped 覆盖率 |
|---|---:|---:|---:|---:|---:|
| Foundation | 3,525 | 10 | 7,360 | 15,568 | 32.45% |
| Geometry | 6,158 | 195 | 5,373 | 5,181 | 54.18% |
| Modeling | 2,965 | 573 | 11,492 | 8,978 | 23.54% |
| MeshData | 139 | 22 | 138 | 235 | 53.85% |
| Mesh | 177 | 2 | 311 | 572 | 36.53% |
| Documents | 1,473 | 126 | 2,339 | 3,337 | 40.6% |
| DataExchange | 7,628 | 51 | 7,997 | 8,998 | 48.99% |
| Xde | 508 | 132 | 728 | 566 | 46.78% |
| Visualization | 4,096 | 116 | 3,272 | 4,106 | 56.28% |
| IVtk | 0 | 0 | 0 | 47 | None% |
| Draw | 0 | 0 | 19 | 1,759 | 0.0% |

[统计快照](PROGRESS_PREVIEW_44_COUNTS.json)以 Emitted+Manual+Blocked 为非 Skipped 分母。
39,029 Blocked 是候选池，AU 完成不等于所有 SDK 声明迁移。
外部 CAD/AS 扩展互操作、Hosted CI、签名、push、公开 NuGet 和整下游验收：NOT RUN。
