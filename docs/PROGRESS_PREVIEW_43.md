# Progress review: Preview.43 (2026-10-08)

AP、AQ、AR 已交付；AS 的核心实现与完整本地检查通过，最终包/源码身份由
OcctSharp/artifacts/batch-as/delivery-result.json 冻结，只有 PASS 才计为交付。
AS 冻结后本轮完成 4/6，余下 AT、AU 两批继续按授权推进。

116,272 个声明：26,602 Emitted、1,227 Manual、39,096 Blocked、49,347 Skipped，Pending=0。
AS 新增 62 个 generated StableId 与单独 9 个 private Manual StableId；本轮四批
共增加 191 generated、9 Manual。声明覆盖不等同产品完成；32 个头文件仍解析失败。

[模块统计](PROGRESS_PREVIEW_43_COUNTS.json)的分母为 Emitted+Manual+Blocked。

| 模块 | Emitted | Manual | Blocked | Skipped | 非 Skipped 覆盖率 |
|---|---:|---:|---:|---:|---:|
| Foundation | 3,525 | 10 | 7,360 | 15,568 | 32.45% |
| Geometry | 6,158 | 195 | 5,373 | 5,181 | 54.18% |
| Modeling | 2,965 | 573 | 11,492 | 8,978 | 23.54% |
| MeshData | 139 | 22 | 138 | 235 | 53.85% |
| Mesh | 177 | 2 | 311 | 572 | 36.53% |
| Documents | 1,473 | 126 | 2,339 | 3,337 | 40.6% |
| DataExchange | 7,561 | 51 | 8,064 | 8,998 | 48.56% |
| Xde | 508 | 132 | 728 | 566 | 46.78% |
| Visualization | 4,096 | 116 | 3,272 | 4,106 | 56.28% |
| IVtk | 0 | 0 | 0 | 47 | None% |
| Draw | 0 | 0 | 19 | 1,759 | 0.0% |

依照[AP–AU 计划](NEXT_ROUND_AP_AU_PLAN.md)，AT 完成交换头/会话筛选，AU 完成
选择工作区及真实窗口集成验收。39,096 Blocked 是候选池，不能据此推算全部 SDK
剩余批次数或声称这两批全部迁移。完整语义扫描仍有解析排除。
AS 扩展持久化值需要 OcctSharp 驱动，普通 SDK/外部 CAD 可能只读到安全子集；
外部 CAD 互操作、Hosted CI、签名、push、公开 NuGet 和整下游应用验收：NOT RUN。
