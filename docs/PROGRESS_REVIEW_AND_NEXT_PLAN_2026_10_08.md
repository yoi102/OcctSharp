# Progress review and next plan (2026-10-08)

本次依据当前 STATUS、各批 delivery-result、Preview.45 库存统计及其输入哈希核对。
原进度复核没有新增迁移准入或产品实现。用户随后要求将规划全部完成；
AU 收尾及 AV–AY 四个大批次现已授权连续执行，先完成 AU 交付再扩大迁移。

后续执行更新：AU 完整本地检查已经通过；最终冻结 PASS 后为 6/6，随后执行 AV–AY。
以下数量/失败记录保留本次最初复核时的快照，最新交付以 STATUS 和 delivery-result 为准。

AV 后续更新：AU 已冻结 PASS；AV 完整本地检查通过，最终 AV 冻结 PASS 后为 2/5。AW–AY 三批继续执行。

AW 后续更新：AU/AV 已冻结 PASS；AW 完整本地检查通过，最终 AW 冻结 PASS 后为 3/5。AX、AY 两批继续执行。

AX 后续更新：此前批次均已冻结 PASS；本批完整本地检查通过，最终冻结 PASS 后为 4/5，剩余 1 批。

AY 后续更新：此前批次均已冻结 PASS；本批完整本地检查通过，最终冻结 PASS 后为 5/5，剩余 0 批。

## 当前完成多少

本轮 AP–AU 六批的功能代码均已写入；**5/6 已完整本地交付，AU 交付未完成**。
已交付基线 Preview.44 / ABI 1.87 / bridge 0.95.0；开发候选 Preview.45 /
ABI 1.88 / bridge 0.96.0。批次交付比例 83.33%，不代表完整 SDK 的完成度。

| 批次 | 内容 | 新增 generated / Manual StableId | 当前结果 |
|---|---|---:|---|
| AP | 拥有的建模接收器、复制形体和诊断 | 66 / 0 | Preview.40，PASS |
| AQ | 二维裁剪与扫描线工作区 | 27 / 0 | Preview.41，PASS |
| AR | 网格数据投影、受控网格化及诊断 | 36 / 0 | Preview.42，PASS |
| AS | Documents/XDE 属性、图层、注释投影 | 62 / 9 | Preview.43，PASS |
| AT | 文件会话、交换头信息和筛选诊断 | 67 / 0 | Preview.44，PASS |
| AU | 选择工作区、来源身份、真实 HWND 输入和六批集成 | 41 / 0 | Preview.45，INCOMPLETE |

已交付五批合计 **258 generated + 9 Manual = 267 项**。
包含 AU 已实现但未交付部分，合计 **299 generated + 9 Manual = 308 项**。
这些是精确 SDK 声明变化数，不是功能按钮数或独立产品能力数。

## 整体和模块覆盖

当前库存 116,272 项：26,710 Emitted、1,227 Manual、38,988 Blocked、49,347 Skipped。
实现覆盖合计 **27,937 项**，包含 AU 尚未交付的 41 项。
非 Skipped 分母为 66,925，声明覆盖率 **41.74%**；全库存覆盖率 **24.03%**。
在相同统计口径下，不含 AU 的已交付基线覆盖 27,896 项，非 Skipped 覆盖率约 41.68%。

| 模块 | 当前已覆盖 Emitted + Manual | Blocked 候选池 | 非 Skipped 覆盖率 |
|---|---:|---:|---:|
| Foundation | 3,535 | 7,360 | 32.45% |
| Geometry | 6,353 | 5,373 | 54.18% |
| Modeling | 3,538 | 11,492 | 23.54% |
| MeshData | 161 | 138 | 53.85% |
| Mesh | 179 | 311 | 36.53% |
| Documents | 1,599 | 2,339 | 40.60% |
| DataExchange | 7,679 | 7,997 | 48.99% |
| Xde | 640 | 728 | 46.78% |
| Visualization | 4,253 | 3,231 | 56.83% |
| IVtk | 0 | 0 | 不适用；47 项为可选依赖排除 |
| Draw | 0 | 19 | 0%；当前不是产品优先方向 |

38,988 Blocked 是待审计池，**不是全部承诺迁移的工作量**。
49,347 Skipped 包含非公开、运算符、类型元数据、测试工具和可选依赖等，不直接转成待办。
32 个 parser exclusions 保留，完整语义扫描未完成；未发现声明也没有可靠的数量分母。
因此不能据此估算整个产品还需多少批或多少天。Pending=0 仅表示已发现声明已分类。

所有权/构造相关的两个最大阻塞原因分别为 NonTransientReceiverOwnership 14,778
和 NonTransientValueConstruction 5,509；通用规则有跨家族收益，但不能自动准入这些声明。
建模覆盖率最低且 Blocked 池最大；优先按实际 CAD 工作流补缺口，不能按内部家族的
声明数量排名直接公开 BOPDS/TopOpeBRep 或可变内核结构。

## 还差多少交付工作

本轮还差 **1 批 AU**。当前证据分别是：

- Release、Debug、冷源码构建通过；Generator 各 263/263。
- 原完整 Runtime 证据各 1024 通过 / 1 失败 / 0 跳过；清理后 Release、Debug
  AU 定向测试各 9 通过 / 1 失败。没有将失败跳过或降级。
- 实际 Debug 原生加载的 62 DLL 路径/哈希通过；229 生成文件冷生成字节一致。
- ABI、布局、寿命、私有头、导出和托管 API 检查已通过。
- 14 个候选包已打包和审计；直接模块消费者通过，Facade 消费者在桌面捕获处失败。
- 真实 HWND 点击/矩形选择、XCAF 保存重开、复制来源身份、尺寸变更失效已通过。
- 真实可见重绘未证明；最终统一交付检查和完整交付冻结未完成。

DXGI 0x887A0027 在没有 Viewer 的独立 GDI 窗口上同样复现；普通 GDI 屏幕颜色变化
可以读取。Windows Graphics Capture 的监视器帧裁剪未得到选择前后像素变化，
HWND 捕获无帧。不能仅凭超时判断 AU 的选择逻辑或显卡驱动有缺陷。

## 执行顺序

### P0：收尾 AU，保持当前授权范围

1. 把窗口呈现、实际物理像素坐标、消息循环和捕获分别做对照；对比正常 GUI 启动
   路径与测试宿主，先确认能取得包含模型的初始可见帧。
2. 通过实际 HWND 点击和矩形输入，证明选择前后的可见像素变化；同一流程验证
   精确来源身份和尺寸失效。普通色块只作采集对照，不能代替模型验收。
3. 修复后按影响复验，再完成完整 Release/Debug、实际 Debug-native、冷源码生成、
   两个干净包消费者和统一 local release-check；复用仍然匹配的独立证据。
4. 更新最终库存/文档/包身份，冻结 AU delivery-result=PASS，才计为 6/6。

### 后续四个大批次：AUTHORIZED / PENDING AU DELIVERY

以下是已授权的下一轮范围。AU 交付通过后，按新基线冻结精确候选和准入，
连续执行 AV → AW → AX → AY，无需逐批再次确认。不预留版本号，
也没有承诺覆盖所有剩余 SDK。准入仍以实际来源、符号、所有权和收益审计为准。

| 顺序 | 大批次 | 内容与实际调用结果 | 准入/退出要求 |
|---|---|---|---|
| AV | 通用所有权规则与 Foundation 值映射 | 审计字符串、路径值、颜色/单位及必要集合缺口；扩展可复用私有接收器/复制输出规则，为后续工作区提供共同支撑 | 核对 TCollection_AsciiString/ExtendedString、OSD_Path、Quantity_Color、UnitsAPI 的现有 public API；禁止重复包装或引入全局可变单位状态；覆盖 Unicode、空值、初始化、释放后复制独立性 |
| AW | Geometry + Modeling 工作流扩展 | 审计剩余样条/填充工作区与受控拓扑组装；曲线/曲面 → 独立形体 → 诊断/修复 → STEP/IGES 回读 | 从 BSplCLib、GeomFill_AppSurf/AppSweep、BRep_Builder 的实际缺口起步；复用 AP/AQ/AM；可变输入隔离、失败不发布结果；拓扑历史多路径不得猜测 |
| AX | Documents/XDE + DataExchange 批量投影 | 审计颜色、材料、视图/尺寸关联及装配交换缺口；批量属性 → 事务/撤销 → 保存重开 → 装配导出回读 → 来源身份核对 | 核对 XCAFDoc_ColorTool/VisMaterialTool/ViewTool/DimTolTool、STEPCAFControl 现有覆盖；复用 AS/AT；错文档、错会话、重复实例和保存持久化必须通过 |
| AY | 网格剩余投影与整链路交付审计 | 审计离散边界、网格关联和循环构造的公开缺口；精确形体 → 受控网格 → 复制边界/关联 → 独立结果和消费者验证；同时核对本轮所需 SDK 前置项 | 从 Poly_Polygon3D/PolygonOnTriangulation/MakeLoops 的有用缺口起步；复用 AR 和现有网格编辑；不暴露裸节点/可变引用；32 排除项逐项保留来源，不虚构缺失 SDK 头文件 |

每个批次覆盖准入审计、生成规则、实现、边界/寿命测试、真实工作流、库存差量、
两个消费者和完整本地交付。相关前置项并入所属大批次，不按单个类型拆成零碎批次。
具体迁移数量由来源、符号、所有权和收益审计决定，没有固定声明配额。

当前明确剩余 **AU 收尾 1 批 + AV–AY 四个已授权大批次**。
这 4 批是下一轮规划，不是完整 OCCT 迁移剩余总数。

外部 CAD/AS 扩展互操作、整下游应用验收、Hosted CI、签名、push 和公开 NuGet：
NOT RUN，分别保留状态，不计入本地包检查的通过结果。

参照：[当前状态](STATUS.md)、[模块统计](PROGRESS_PREVIEW_45_COUNTS.json)、
[当前 AP–AU 计划](NEXT_ROUND_AP_AU_PLAN.md)、[执行政策](WORKFLOW.md)。
