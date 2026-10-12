# BY：布尔、修复与完整历史消费流程

状态：IMPLEMENTED / LOCAL GATES PASS; FINAL FREEZE REQUIRED；[当前实现合同](BATCH_BY_BOOLEAN_REPAIR.md)。
用户已授权 BR–CI 连续完成；BY 实现以 BX 的 delivery-result.json PASS 为前置条件。
本文件不单独关闭 BY；BX 的冻结 PASS 历史权威保持不变。

固定 SDK 提交为 `b8f597c677811d1f9f4d8a97f5ae2825c0353a42`。
预审已读取该提交的 20 个源文件，筛出 23 条现有 Manual 签名与八个相关 caller。
原始签名、源码哈希和范围草案位于
[只读预审](../OcctSharp/artifacts/batch-by/read-only-preaudit.json)与
[范围草案](../OcctSharp/artifacts/batch-by/scope-proposal.json)。
BX 已冻结 PASS；真实 Preview.73 基线与21固定源码已核对并建立
[正式准入](BATCH_BY_BOOLEAN_REPAIR_ADMISSION.json)。原始草案保留为历史输入。

## 拟准入的生成规则

TM058 BooleanRepairAdapters 已处理 19 条 Manual→Emitted，新增 SDK 身份预期为 0。
准入针对 BooleanTopologyHistory.cpp 和 RepairExecution.cpp 的完整调用流程，
不宣称其他布尔/特征 caller 实例均已转换。

| 家族 | 拟转换 | 边界 |
|---|---:|---|
| Fuse/Cut/Common 默认构造、布尔 Build、参数/工具组和非破坏模式 | 7 | 已初始化的私有 builder；保留已有 input graph、选项和发布行为 |
| builder Modified/Generated/IsDeleted/Shape | 4 | 复制拥有式拓扑列表和值；保留每个来源与目标，不返回 SDK 借用引用 |
| BOPAlgo 错误/警告读取、DumpErrors/DumpWarnings、局部并行选项 | 5 | 私有 ostream 复制；每类诊断最多 65536 字节，溢出整体失败 |
| BRepTools_History 的 Modified/Generated/IsRemoved | 3 | 已有非空私有 repair-history owner；复制列表，不暴露 mutable history |

四条其他路径的双形状 Fuse/Cut 构造、Fuzzy 与 Glue 不在这两个 caller 的完整准入内，
继续列为待审，保留其现有实现。SetToFillHistory 复用已 Emitted 支撑调用；
继承的 IsDone 仍按现有 Manual 支撑例外记录，不给新增迁移信用。

预审还确认：native-header-only emitter 当前直接计入所有 selected 支撑行。
如果本批将 Manual IsDone 纳入显式 supporting 合同，应与常规 emitter 一致，
从 SourceStableIds 信用中排除仍为 Manual 的支撑行，并增加准确计数/漂移回归。
此生成器修正已实施，回归测试与完整生成器套件通过。

## 完整结果与身份合同

复用已有 BooleanHistoryModeling、ShapeRepair 和 AJ repair workspaces。
补齐公开消费者中的“批量布尔→诊断→修复→后续编辑→STEP 重开→实际显示”链。
现有能力不重复计作新增 SDK 声明；只有确实缺失且通过正式准入的行为才新增。

布尔的来源属于 PlanId/argument slot，目标属于该结果的完整拓扑图；修复选择属于
RepairIdentity/revision。进入新的快照时须有实际 copier 的精确对应证据。
没有证据就保持 unavailable/unknown。保留 split、共享目标、删除、多个后继和
所有来源路径，不按几何相似度选择目标，不把文件重开后的拓扑当作旧身份。
本批不预先承诺通用持久命名或自动选择重绑定。

错误和警告在 builder 存活期间复制到已有 owning result 的 Message。
SDK 失败继续拒绝 RequireShape，诊断可独立留存；预算失败不得发布部分结果。
复制 TopoDS 列表保留私有图中的真实身份，不冒充几何 deep copy；输入图的现有
deep-copy 与结果所有权合同仍负责源隔离。

## 验证与交付

1. 冻结 BX 基线后，完成 exact admitted/deferred/reused/prerequisite 清单和 caller 等价审计。
2. 修改合同、模板与 emitter 后再生成；核对既有输出字节、19 条增量及支撑信用。
3. Release/Debug 原生探针对照 SDK 调用、列表独立生命周期、真实 SDK 失败诊断与流预算拒绝。
4. Runtime 和两个干净消费者覆盖 Fuse/Cut/Common、多个参数/工具组、split/共享/删除、
   实际合并和修复历史；核对源不变、失效身份拒绝和独立结果。
5. 同一消费者结果必须有实际文件几何和 HWND/DXGI、拾取身份、重绘/失效证据。
   可见窗口测试串行运行；主机计时不作为物理显示或输入延迟证据。
6. 保留 WORKFLOW 的完整绑定交付门禁。冻结 PASS 后再进入 BZ。

后续顺序仍为 BY→BZ→CA→CB→CC→CD→CE→CF→CG→CH→CI，不重新询问常规执行许可。
Hosted CI、签名、push、公开 NuGet、外部 CAD、物理设备及完整下游应用验收分别按实证记录。
