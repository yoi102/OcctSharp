# Preview.61 整体进度复核（2026-10-11）

当前本地交付：Preview.61 / ABI 1.104 / bridge 0.112.0。
BF–BK 六批冻结 PASS；原授权规划剩余 0。本次是进度复核与下一轮规划，未实施新绑定。
依据：[当前状态](STATUS.md)、[复核统计](PROGRESS_PREVIEW_61_REVIEW_COUNTS.json)、
`OcctSharp/artifacts/batch-bk/delivery-result.json` 和各批冻结记录。

## 本次核验与已完成工作

读取最终 inventory，并按 discovery、BF 补充发现和模块分类规则重新连接统计；
六个统计输入哈希全部匹配，库存哈希与 BK 冻结交付一致。
六批的证据文件及每批 14 个包的哈希全部匹配。
本次没有重新生成、编译、Runtime 或打包：NOT RUN。下面的测试结果复用冻结交付证据。

| 批次 | 新增 generated StableId | 本地版本 | 冻结结果 |
|---|---:|---|---|
| BF | 11 | 8.0.1-preview.56 | PASS |
| BG | 27 | 8.0.1-preview.57 | PASS |
| BH | 16 | 8.0.1-preview.58 | PASS |
| BI | 39 | 8.0.1-preview.59 | PASS |
| BJ | 13 | 8.0.1-preview.60 | PASS |
| BK | 16 | 8.0.1-preview.61 | PASS |

本轮累计 122 个 generated StableId，Manual 增量 0；解析排除由 32 减至 28。
BF 新发现 94（34 Blocked、60 Skipped）与新增迁移分开计数。
Release/Debug/冷源码 Generator 302、Runtime 1257；实际 Debug-native 1257 和
62 个实际加载 DLL；291 个确定性生成文件；14 包、两个干净消费者、ABI/API/导出、
真实 HWND/DXGI 保存重开与 identity/stale 验收均以 BK 冻结 PASS 为证据。

## 整体数量与剩余边界

已分类 116,366 个声明：Emitted 27,276、Manual 1,227，合计已覆盖 28,503。
Blocked 38,456；Skipped 49,407；Pending 0。
非 Skipped 分母 66,959，覆盖率 42.5678%，其余 Blocked 比例 57.4322%。
对全部已发现声明的覆盖率是 24.4943%。两种比例均为声明覆盖，不是 CAD 产品完成率。

38,456 个 Blocked 包含内部接口、有意暂缓及当前不安全的绑定，并非全部承诺迁移。
49,407 个 Skipped 包括 operator、类型元数据、析构、非公开及测试接口等，不能算作待做队列。
28 个解析排除项仍存在，未发现声明没有已知分母；完整语义扫描未完成。
因此不能给出完整 SDK 还需多少批次、总功能完成百分比或总完成时间。

## 各模块

| 模块 | Emitted + Manual | Blocked | 非 Skipped 声明覆盖率 |
|---|---:|---:|---:|
| Foundation | 3,718 | 7,177 | 34.13% |
| Geometry | 6,429 | 5,331 | 54.67% |
| Modeling | 3,610 | 11,420 | 24.02% |
| MeshData | 175 | 124 | 58.53% |
| Mesh | 183 | 307 | 37.35% |
| Documents | 1,666 | 2,272 | 42.31% |
| DataExchange | 7,710 | 7,966 | 49.18% |
| Xde | 759 | 609 | 55.48% |
| Visualization | 4,253 | 3,231 | 56.83% |
| IVtk | 0 | 0 | — |
| Draw | 0 | 19 | 0.0% |

IVtk 没有本次统计中的非 Skipped 分母；Draw 的 19 项来自 QABugs，均不列入当前 CAD 产品迁移主线。
Modeling/DataExchange/Foundation 的 Blocked 合计 26,563，占 69.07%，为下一轮重点。
全库主要阻塞是接收器所有权 14,502、值构造 5,488、未映射值类型 5,114、
借用/输出引用 4,975、裸指针寿命 3,547；规则改进必须保持初始化和所有权证明。

## 下一步与交付状态

[BL–BQ 规划](NEXT_ROUND_BL_BQ_PLAN.md) 提出六个较大批次，399 个原始家族候选。
状态 PROPOSED，尚未实施；候选数量不是准入数。每批围绕完整可验收工作流组织。
本次新增规划/复核文档不属于已冻结 Preview.61 包，不改写旧交付记录；
下一次产品交付时重新打包并核对文档内容。

源码未提交或推送。Hosted CI、签名、公开 NuGet、外部 CAD 互操作及完整下游应用验收：NOT RUN。
本地交付完成、完整 SDK 迁移、外部验收和公开发布分别报告。
