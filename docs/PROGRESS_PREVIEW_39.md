# Progress review: Preview.39 (2026-10-07)

## 当前结论

当前开发版本是 **8.0.1-preview.39 / ABI 1.82 / bridge 0.90.0**。
最近两轮 AF–AK 六批、AL–AO 四批均完成本地交付：**10/10，旧计划剩余 0 批**。
当前已经具备多模块 CAD 工作流和可重复的本地交付验证，但全量 SDK 迁移尚未完成。
公开发布及完整下游应用验收仍是独立关卡。

[STATUS](STATUS.md) 是当前状态入口；本报告给出模块分布和统计口径。
[下一轮 AP–AU](NEXT_ROUND_AP_AU_PLAN.md) 是新规划，共六批，实施尚未开始。

## 整体库存及剩余量

| 最终分类 | 数量 | 含义 |
|---|---:|---|
| Emitted | 26,411 | 已生成绑定的原生声明 |
| Manual | 1,218 | 已登记的手工绑定声明 |
| Blocked | 39,296 | 已发现、当前未通过生成条件；需按用途与风险审计 |
| Skipped | 49,347 | 按现行策略跳过；不是默认待开发队列 |
| Pending | 0 | 已发现声明的最终分类无未决项 |
| 合计 | 116,272 | 当前库存可计数的声明 |

已绑定 **27,629** 项。两个声明覆盖口径分别是：

- 全库存：27,629 / 116,272 = **23.76%**。
- 去掉 Skipped：27,629 / 66,925 = **41.28%**；此口径的 Blocked 比例为 **58.72%**。

这些都是**声明覆盖率，不是产品完成率**。一个完整 CAD 功能可能通过少量高层 API
实现，也可能依赖许多底层声明。Emitted、Manual、导出符号和托管 API 的数量不能互换。
39,296 项 Blocked 包含低层内核、内部状态和长期暂缓项，不能承诺全部迁移。
49,347 项 Skipped 中包括运算符 21,564、类型元数据 11,603、非公开成员 4,522、
析构生命周期 4,182 等；它们不应全部计为“还差多少功能”。

完整扫描的 `IsComplete=false`，仍有 **32 个解析失败**。最终分类完成、Pending=0
只针对已发现声明，不能证明这些失败头文件中的声明已经发现或覆盖。
未发现部分没有可靠数量，因此当前不能给出全 SDK 的最终完成百分比、剩余总批次数或日期。

## 各模块进度

下表覆盖率 = (Emitted + Manual) / (Emitted + Manual + Blocked)。
“现有能力”概括已交付接口和历史工作流，并不表示该模块全量 SDK 已完成。

| 模块 | Emitted | Manual | 已绑定合计 | Blocked | Skipped | 声明覆盖率 | 现有能力与主要差距 |
|---|---:|---:|---:|---:|---:|---:|---|
| Foundation | 3,524 | 10 | 3,534 | 7,361 | 15,568 | 32.44% | 值映射、复制数组/序列/网格、数值矩阵与求解；剩余主要是指针、引用、字符串和非瞬态对象 |
| Geometry | 6,131 | 195 | 6,326 | 5,400 | 5,181 | 53.95% | 曲线/曲面构造评估、插值、样条缓存及系数；剩余是私有算法工作区、构造及耦合输出 |
| Modeling | 2,899 | 573 | 3,472 | 11,558 | 8,978 | 23.10% | 布尔、局部特征、分区、修复、拓扑集合和精确历史；剩余接收器与构造最多，含大量内核内部结构 |
| MeshData | 110 | 22 | 132 | 167 | 235 | 44.15% | 复制网格、属性、索引和对应关系；剩余 Poly 容器、邻接和可变引用 |
| Mesh | 171 | 2 | 173 | 317 | 572 | 35.31% | 网格化、LOD、编辑、焊接及离散模型交付；剩余 BRepMesh 参数、诊断和内部算法 |
| Documents | 1,448 | 120 | 1,568 | 2,370 | 3,337 | 39.82% | OCAF 事务、撤销/重做、持久化、参数化重算和命名；剩余属性容器、标签投影、存储工具 |
| DataExchange | 7,561 | 51 | 7,612 | 8,064 | 8,998 | 48.56% | STEP/IGES、私有读写会话、转移记录及不可变诊断；剩余会话/实体/Writer 接收器与映射 |
| Xde | 471 | 129 | 600 | 768 | 566 | 43.86% | 装配编辑、BOM、实例位置、元数据、PMI 和格式往返；剩余工具、标签与属性/注释投影 |
| Visualization | 4,096 | 116 | 4,212 | 3,272 | 4,106 | 56.28% | Viewer、输入、选择、操纵器、标注、渲染及帧捕获；剩余选择体、控制器和渲染上下文寿命 |
| IVtk | 0 | 0 | 0 | 0 | 47 | 不适用 | 可选 VTK 依赖排除，未纳入当前交付承诺 |
| Draw | 0 | 0 | 0 | 19 | 1,759 | 0.00% | 原生测试/命令设施；19 项 QABugs 引用接口暂缓，不是 CAD 应用核心优先项 |

Runtime、Native、Facade 属于运行/交付层，没有可比的 OCCT 声明分母：

- Runtime：加载、错误、ABI、所有权和寿命基础已通过当前本地关卡。
- Native：单一桥接 DLL，当前运行时 62 个 DLL，41,267 个原生导出，Debug 导出一致。
- Facade：4,214 个类型转发，干净 facade 消费者通过；托管 API 清单共 56,413 项。
- 交付共 14 包：Native、Runtime、11 个领域模块和 facade；当前平台为 Windows x64 / .NET 10。
  OpenGles 没有当前独立包或可比库存分母，不能从上表推断新平台支持。

## 剩余工作的结构

| Blocked 原因 | 数量 |
|---|---:|
| NonTransientReceiverOwnership：非瞬态接收器所有权 | 14,960 |
| NonTransientValueConstruction：非瞬态值构造 | 5,527 |
| UnmappedValueType：未映射值类型 | 5,249 |
| BorrowedOrOutputReference：借用/输出引用 | 5,048 |
| RawPointerLifetime：原始指针寿命 | 3,572 |
| UnselectedHandleTarget：未选择的 Handle 目标 | 2,577 |
| RValueReferenceTransfer：右值转移 | 1,277 |
| TemplateInstantiationProjection：模板实例投影 | 515 |
| MissingToolkitProvenance：缺少 Toolkit 来源 | 445 |
| UnverifiedFreeFunctionExport：自由函数导出未证明 | 117 |
| CallableEligibilityInvariant | 8 |
| CrossModuleProjectionContract | 1 |

优先补可复用的接收器/复制输出规则，随后按完整调用流程扩展各领域；
不能通过放开原始指针或把内部可变上下文直接公开来提高覆盖率。

[AO SDK 账本](BATCH_AO_SDK_BLOCKERS_LEDGER.json) 记录 **33 个配置排除头文件**：
VTK 19、EGL/GLES 1、RapidJSON 1、C++/CLI 1、缺少 SDK 生成前置文件 10、
可解析但刻意私有的 lexer 接口 1。前五组形成 32 个解析失败。
另有 17 个配置中的缺符号排除和两项历史 BRepCheck 符号暂缓。
这部分需要真实 SDK/导出证据；本轮不虚构补齐头文件或扩大平台承诺。

## 已完成交付及未运行关卡

AL 完成复制矩阵/私有求解，AM 完成样条工作区，AN 完成 Writer 诊断，
AO 完成 SDK 排除证据和可重复交付工具。本轮新增 **89 个 Emitted ID**：42 + 31 + 16；
AO 没有迁移增量。

冻结的 Preview.39 证据位于
[`delivery-result.json`](../OcctSharp/artifacts/batch-ao/delivery-result.json)：

- Release、Debug、冷源码 Release 各通过 **238 Generator / 924 Runtime**，无失败或跳过。
- 隔离的实际 Debug-native **924/924**，62 个实际加载 DLL 的路径和哈希通过。
- 204 个生成文件及 manifest 字节一致；依赖闭包完整且无环。
- C11 头文件、布局、私有头文件独立编译和释放并发检查通过。
- 14 个包、两个干净消费者、构建/运行时/包/消费者的负载身份通过。
- 13 个快照 fixture、真实并发 bootstrap、缺桥接恢复、失败释放锁通过；
  真实并发文档变更被拒绝，未提升任何候选包；1,587 个输入已冻结。

**NOT RUN**：Hosted CI、签名、push、公开 NuGet、完整下游应用验收。
源码尚未提交。以上为已有冻结报告，本次规划没有重新运行产品构建和测试。
新规划与本报告尚未进入 Preview.39 的冻结包，需在下一次交付时重新打包验证文档。

## 统计可重现性

[统计 JSON](PROGRESS_PREVIEW_39_COUNTS.json) 绑定最终库存、发现模型、冻结交付报告、
模块分类器和统计脚本的 SHA-256。按 StableId 联结，保持全部 116,272 行。
发现模型有 116,263 行；剩余九项是 ExprIntrp 私有 yacc 自由函数，
按当前分类器 `Expr` 前缀归 Foundation，全部保持 Blocked，逐项记录回退来源。
这些归属补充没有改变生成器分类或迁移状态。

从仓库根目录，用可用 Python 执行：

```powershell
python OcctSharp/eng/report-migration-progress.py `
  --inventory OcctSharp/artifacts/generator-reports/full-inventory.json `
  --discovery OcctSharp/artifacts/generator-determinism/gp-pnt-discovery-1.json `
  --delivery OcctSharp/artifacts/batch-ao/delivery-result.json `
  --output docs/PROGRESS_PREVIEW_39_COUNTS.json `
  --ap-candidates docs/BATCH_AP_MODELING_CANDIDATES.json
```

脚本拒绝冻结库存哈希不符、重复 ID、未经审计的模块回退及无法对齐的总量。
`coverage.json` 的前置模型统计和重叠状态不作为本报告的最终覆盖口径。
