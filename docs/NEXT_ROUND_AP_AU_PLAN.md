# Next round: AP–AU after Preview.39

规划日期：2026-10-07。基线：本地交付 Preview.39 / ABI 1.82 / bridge 0.90.0。
状态：**AUTHORIZED / IN PROGRESS**。用户已要求全部完成六批。
AP–AT 已交付；AU 完整本地检查已通过，最终冻结 PASS 后为 6/6；每批保留准入审计及完整本地交付。
原 AF–AK 和 AL–AO 全部交付，本轮是新的六批，不是历史遗留队列。

[进度报告](PROGRESS_PREVIEW_39.md) 和 [统计快照](PROGRESS_PREVIEW_39_COUNTS.json)
提供全量/模块分母；[STATUS](STATUS.md) 维护实际实施和交付状态。

## 规划原则

39,296 项 Blocked 是候选池，不是六批的承诺交付量。没有固定能力配额或版本预留。
按有用结果、依赖与所有权风险形成大批次，每批包含准入审计、规则、生成、运行语义、
真实工作流、两个包消费者和完整本地交付。开发中允许按 WORKFLOW 使用已证明的前置项，
但完成当前批次交付后才扩大迁移范围。

先审计现有 public API 和 pinned SDK，避免重复现有事务、装配、网格、修复和样条能力。
源码/符号/复制独立性不满足的候选保留逐项 deferred 原因，不降低验收要求。
扩展已有所有权规则需证明构造初始化、接收器有效性、输入保留和结果独立性；
新增边界在发射前按 DECISIONS/OWNERSHIP/TYPE_MAPPING 形成必要 ADR。

## 六个完整批次

| 顺序 | 新增重点与候选起点 | 完整调用流程 | 准入边界和验收 |
|---|---|---|---|
| **AP：建模接收器与复制诊断** | BRepLib_MakeEdge/MakeWire、ShapeAnalysis_ShapeContents、ShapeAnalysis_Wire 剩余引用输出；同时复用 Foundation 的已审计值/复制规则 | 拥有的曲线与点 → 私有 Edge/Wire 构造器 → 独立拓扑结果 → 内容及局部诊断快照 → 复用修复 → STEP/IGES 回读 | 明确未完成构造器的 Error/IsDone；禁止返回内部 Edge/Wire 引用；错误不发布结果；源/接收器释放后的输出可用；真实断连、退化和修复案例 |
| **AQ：二维裁剪/扫描线几何工作区** | Geom2dHatch_Hatcher 及其必要 intersector/domain 前置项；依据流程审计参数区间、曲线事件和复制数组 | 有向 2D 边界与扫描线 → 私有几何工作区 → 复制交点/区间及失败状态 → 独立裁剪曲线 → 复用 AP 拓扑构造/面域验证 | 区分切触、重合、多个区间、空结果和失败；参数/方向/容差明确；不得泄露 ChangeIntersector 等借用状态；用解析几何参照证明区间，复用现有样条评估 |
| **AR：网格数据与网格化诊断规则** | Poly_Triangulation/Poly_Connect、节点与 UV 容器、BRepMesh 参数及状态；复用 R 的编辑、邻接、属性和对应关系 | 精确形体 → 受控网格化 → 复制节点/UV/邻接/状态 → 复用网格编辑 → 独立离散形体/格式回读 | 不重复实现现有编辑和 LOD；验证空网格、索引界限、退化/非流形、参数状态和源释放；裸节点地址与可变三角形引用暂缓 |
| **AS：Documents/XDE 属性与注释投影** | TDataStd_NamedData、TDF_Tool、XCAFDoc_LayerTool/NotesTool 及必要标签/属性映射；精确选择现有 public API 未覆盖的类型 | 文档绑定的批量属性/图层/注释操作 → 命名事务 → 复制关联和诊断 → 撤销/重做 → 保存重开 → 重复装配实例检查 | 复用现有事务、BOM、PMI 和参数重算；标签跨文档/失效校验；不存在值与零值区分；回滚不留半写属性；不公开可变 TDF/XCAF 容器 |
| **AT：交换头信息与会话筛选诊断** | APIHeaderSection_MakeHeader、IFSelect_WorkSession 以及所需 STEP/IGES header/entity/check 投影 | 私有文件会话 → 审计后的头信息与实体筛选 → 不可变选择/诊断快照 → 复用 AK/AN 转移写出 → 独立回读 | 不重复 AN WriterTransferSession；错会话/错实体域拒绝；导出后核对实际支持的头信息；外部可变 actor/model、回调和任意 raw writer mutation 暂缓 |
| **AU：选择工作区与集成消费者验收** | SelectMgr_SelectingVolumeManager、AIS_ViewController/V3d_View 的实际缺口；复用 ViewerInputController、ViewerSelectionItem 与帧捕获 | 新增选择/输入能力 → 真实 HWND 路径 → 复制选择与来源身份 → 修订后失效处理 → 串联本轮模型/网格/文档/交换消费者 | 严格创建线程/Viewer 寿命、矩形/射线范围、对象释放和陈旧 revision；输入事件的视觉结果需实际窗口证据；渲染时间不等同物理显示 FPS；完整本轮集成和交付 |

默认顺序 **AP → AQ → AR → AS → AT → AU**。Foundation 的支撑映射随需要它的批次审计，
不另拆成许多小批。运行基础和 Native/Facade 随每批共同验证。
各批可以在审计后调整内部候选；改变完整目标或扩大平台范围需明确更新计划。

## AP：下一批的具体输入

[冻结候选 JSON](BATCH_AP_MODELING_CANDIDATES.json) 包含 **140 项当前 Blocked**：

| 家族 | 待审计候选 |
|---|---:|
| BRepLib_MakeEdge | 52 |
| BRepLib_MakeWire | 17 |
| ShapeAnalysis_ShapeContents | 52 |
| ShapeAnalysis_Wire | 19 |

这是精确 StableId/签名和来源哈希的审计输入，**不是 140 项全部准入承诺**。
另列已有 59 Emitted、2 Manual 供复用，不计新增迁移。
140 项中 138 项属 Modeling，两个 ShapeAnalysis_Wire RTTI 静态接口按当前模块分类器属
Foundation；它们只在候选全集中保留，实施审计应独立处理，不为扩大数量自动公开。

第一步逐项检查构造初始化、实际 SDK 实现/符号、输入别名、输出所有权和现有接口覆盖，
记录 admitted/deferred/prerequisite/reused，冻结准入后实施。
BRepLib 与已有高层 builder 的重叠必须说明新增生成规则或必要诊断收益；
没有收益或无法证明寿命的接口可以明确暂缓，不通过重复命名包装制造新增功能。
AQ 已冻结 50 候选及 20 准入/30 暂缓/7 新前置；AR 已冻结 75 候选及 35 准入/40 暂缓/1 新前置；AS 已冻结 163 候选，49 generated/2 private Manual/112 deferred，13 新生成前置；AT 已冻结 182 候选（63 准入、119 暂缓、4 新前置、30 复用）；AU 已冻结 213 主候选（39 准入、174 暂缓、2 新前置、19 复用；generated delta41）；各批启动时重新核对其基线。

## 本轮不承诺的范围

- 所有 Blocked/Skipped 都迁移；任意模板、借用存储、零复制、用户回调和全局可变内核上下文。
- IVtk/Draw 全量绑定、添加 VTK 或 EGL、跨平台支持、拆分 Native DLL。
- 用几何相似性猜测拓扑历史；多路径、split/merge 或证据不足必须保留歧义，禁止自动重绑定。
- Cadoryx 的整应用验收、物理显示/输入性能及未运行的平台场景。
- Hosted CI、commit/push、签名和公开 NuGet 发布；这些状态单独记录。

这些边界不阻碍本轮授权后的正常内部实现、依赖处理和本地验证。

## 交付跟踪

每批保留完整 Release/Debug 构建与 Generator/Runtime suites、实际 Debug-native 身份、
生成一致性、依赖/ABI/布局/导出/API/寿命验证、最新最终库存、14 包、两个干净消费者和
统一 local release-check。适用真实文件/窗口案例不可用单纯路由计数替代。
只复用输入身份未变的证据；不因某次中间开发操作重复所有产品关卡。

分别记录候选、准入、暂缓、前置和复用数，以及准确的 Blocked→Emitted StableId 差量。
每批完成后更新 STATUS、模块统计和下一批输入；剩余批数与剩余声明量分别展示。
下一次打包将新增规划/统计文档纳入包内容审计，不覆盖旧 Preview.39 冻结包。

当前：**AU 完整本地检查通过；最终冻结 PASS 后为 6/6**。六批结束后依据新库存和实际缺口再规划；
不声称届时完整 SDK 迁移完成，也不虚构所有剩余工作的总批次数。
