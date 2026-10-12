# BR–CI 长期规划：18批，CI最终冻结时全部完成

日期：2026-10-12（Asia/Tokyo）。冻结前基线：17/18 PASS；CI authority PASS时为18/18完成。
CH Preview.83已冻结；CI Preview.84采用同一ABI1.117/bridge0.132.0，
不新增SDK声明或迁移信用。最后冻结权威为batch-ci/delivery-result.json。

| 范围 | CI冻结时完成 / 总批次 | 结果 |
|---|---:|---|
| BR–BU生成器与基础合同 |4/4|规则、库存、复制容器、数值/诊断|
| BV–BZ几何与建模 |5/5|精确建模闭环；Pinned AppSweep仍deferred|
| CA–CE网格、文档与交换 |5/5|资源/装配/保真矩阵及格式链|
| CF–CI交互、下游与冻结 |4/4|真实输入/context/完整应用/最终身份|
| 合计 |18/18|仅当CI authority PASS；不是SDK整体完成度|

整轮Emitted+653，其中629条原Manual迁移、24条其他分类迁入。
CH41个有界消费者支持生成输出不增加声明信用。库存覆盖42.7396%，
还剩598 Manual/38341 Blocked/49407 Skipped/28解析排除；后续先准入
真实消费工作流，再确定批次规模。完整证据、限制和剩余处置见
[CI整轮冻结](BATCH_CI_ROUND_FREEZE.md)。

## 目标与批次规模

规划覆盖生成器、核心建模、文档交换和实际 CAD 消费者，延长至 18 批。
每批完成一个较完整结果：生成规则/绑定、错误与生命周期、实际消费者、
相关文件或画面证据及交付冻结。相关家族可合并迁移，避免只加几个孤立方法；
不设置最低声明数，不为了做大一批混入无关或未经审核能力。

这是 18 个规划工作包，不是“18 批后整个 SDK 迁移完”。
Blocked 家族只是审计入口，下面不承诺全家族或尚未审计的重载。
每批在启动时核对消费者实际缺口：已有能力优先复用，确实缺失才新增生成合同。
准入不足时记录明确的诊断/审计成果，不把 deferred 宣称已实现。
已有完整结果可通过实际消费者复用来交付，迁移增量允许为零；
仅完成审计不能关闭承诺功能结果的批次。阻塞结果仍列在未完成范围中，
替代路径须满足原有行为验收；无法满足时明确缺口，不自行取消用户要求。

## 生成器优先的贯穿工作线

本轮不仅要求新增绑定走生成器，还要逐批审核相关旧 Manual 实现是否能安全迁移。
BR 审计的剩余1226行尚未完成完整工作流准入，不是“全部不可生成”的结论。
其按模块的待审核范围为：

| 模块 | 待完整准入的 Manual 行 | 配合的批次 |
|---|---:|---|
| Foundation | 10 | BT、BU |
| Geometry | 194 | BT、BV、BW |
| Modeling | 573 | BX、BY、BZ |
| MeshData / Mesh | 22 / 2 | CA |
| Documents | 126 | CB |
| Xde | 132 | CC、CD |
| DataExchange | 51 | CD、CE |
| Visualization | 116 | CF、CG |
| 合计 | 1226 | CH 汇总消费者实际覆盖，CI 汇总最终处置与未完成项 |

这些数字来自 [BR人工绑定审核](BATCH_BR_MANUAL_BINDING_AUDIT.json)，
是待审核库存而非预定迁移配额。按共享的 SPECIAL_CASE/workflow 成组审核，
完整规则可表达且等价验证通过的转为生成；保留项记录具体所有权、线程、
驱动或运行时边界和理由；待解决项保持明确未完成状态。
不能仅以“复杂”或“高层 API”作为永久手写理由，也不能通过改分类清零。

每批分别统计：新规则、Blocked→Emitted、Manual→Emitted、兼容模板迁移、
新增消费者结果及复用。模板由人维护、由生成器输出，其来源应如实说明；
把手写代码搬进模板不自动代表可复用规则扩大或新增 CAD 能力。
输出必须有合同/规则/模板来源与哈希，可重新生成；不在 generated 文件中修补。

## 最近两批的执行清单

### BT：耦合复制容器与多点约束

采用已有 [16个主候选和4个支撑候选](BATCH_BT_COUPLED_COLLECTIONS_CANDIDATES.json)
已完成精确准入；固定源码为 OCCT 提交
`b8f597c677811d1f9f4d8a97f5ae2825c0353a42`；实现审计读取该提交，不能把上游 HEAD 当作基线。

1. 锁定 AppDef_MultiPointConstraint / AppParCurves_MultiPoint 的精确签名、
   初始化、实际链接符号以及对应 CAD 消费路径；区分 admitted/deferred/reused/prerequisite。
2. 建立可复用的行计数/偏移/总预算合同，把嵌套批次的3D/2D位置、切向、曲率
   一起复制。各维度长度分别配对；内部1-based与对外bounds/ordinal映射明确。
   2D SDK getter 的全局索引从3D数量之后开始，缺失通道不得调用相应 getter。
3. 明确 null、空侧、空行、空批次、可选通道与有限值行为；空侧不能强行调用
   不接受空数组的 mixed 构造。call-private对象成功后才发布完整不可变结果，
   不以 SDK 浅拷贝证明脱离源。超预算、溢出和枚举异常均整体失败。
4. 修改规则/合同/native与managed emitter或模板，重新生成并编译；保留
   bounds、行映射、通道状态、错误上下文和生成来源。核对相关 Manual 等价迁移机会。
5. 验证源修改/释放后快照、lazy枚举及枚举器释放、错误原子性、混合索引与预算边界；
   用既有拟合/几何消费者证明约束具有实际用途，必要新增前置项须另外精确准入。
6. 两个干净消费者验证该完整流程，完成绑定交付全部适用门禁，再进入 BU 实现。

BT 中间成果依次为：精确准入→生成合同与规则→真实行为/消费者→冻结交付。
源码审计、成功生成和编译通过各自独立，不能提前把候选20项记为已迁移。

### BU：单位、数值与留存诊断

复用 AV 数值与 BN 有界文本路径，先定位未覆盖的用户输入和诊断需求。
围绕同一流程完成“单位解析/转换→数值校验→几何消费→复制诊断留存”，
把相关值映射、构造和消息读取合同一起处理，不批量暴露无用途的方法。

准入须明确单位维度、精度、全局配置副作用、非有限值和失败分类；
消息仅在已证明的接收者/线程合同下复制成独立快照，不引入通用托管回调。
同时审计 Foundation 的10个相关 Manual 行，区分可生成与必要基础设施。
消费者验证正确转换、维度冲突、非法批量输入及源释放后诊断，
完成 BT+BU 的阶段集成，并保留每批独立冻结与迁移账。

## 阶段一：生成规则与基础合同（BR–BU）

| 批次 | 较完整的目标 | 审计入口与规则工作 | 退出证据 |
|---|---|---|---|
| BR | 建立可追溯的 generated/Manual 边界与复用清单，减少可重复人工绑定 | 对接 manifest、operation contracts、SPECIAL_CASES；选择可被现有 owner/value/collection 规则安全替代的人工例外，保持历史 API/ABI | 代表性迁移等价行为、准确新增/复用/Manual 转换账；生成文件来源可追溯；没有强行改写 Viewer 基础设施 |
| BS | 收敛解析与 SDK 链接排除的真实原因，修复可复用 discovery 缺口 | 实际 28 个 Failures 分组：缺 SDK 头、VTK 依赖、平台/配置与解析问题；安装头、源码、导出符号逐项对照 | 可修复项重扫描/重分类/编译；剩余逐项原因与依赖记录；Optional VTK 不为清零而启用 |
| BT | 复制容器规则覆盖更复杂但可证明的耦合/嵌套数据工作流 | 从 mapped 元素的嵌套数组、成对集合、可选缓冲与跨模块映射缺口选择；复用已有 Array1/Sequence/Array2/map 实现 | null/empty/lower-bound、溢出与预算、失败原子性、释放后快照；native/managed 冷生成一致 |
| BU | 基础值、单位、数值和消息诊断组合为可使用工作流 | 数值/单位值、Message_Messenger 等候选；优先复制消息快照，控制接收者构造和线程边界；避免通用托管回调 | 单位维度/精度、非有限值、批量输入错误、诊断留存及并发释放；阶段一消费者交付 |

BR 先确认 Manual 例外哪些应该保留；BS 发现的新声明不自动获得 emitted 信用。
BT/BU 只选择真实依赖；已经完成的 BN 字符串变换与 AV 数值路径记复用。

## 阶段二：几何与核心建模（BV–BZ）

| 批次 | 较完整的目标 | 审计入口与规则工作 | 退出证据 |
|---|---|---|---|
| BV | 投影、距离、交点和参数域诊断覆盖实际建模缺口 | 复用 BL/BM/已有 intersection；补有限参数域、periodic seam、重合/多解等确实缺失的返回合同；Extrema/投影候选按精确签名审核 | 解析已知解、边界/退化/无解/多解；参数与几何耦合且释放后有效；不承诺全局最优 |
| BW | 样条、拟合与填充工作区补齐未覆盖变体并处理 AppSweep 决策 | BSplCLib、AppDef/Approx/GeomFill 中消费者需要的路径；审核构造初始化与内部缓存；AppSweep 固定源码缺陷单独复现/修复准入 | 采样位置/导数/连续性、权重/节点非法值、源释放；缺陷无法安全修复则明确 deferred 和可行范围 |
| BX | 曲线到边/线框/面的精确拓扑装配与验证闭环 | 复用 BL/AP/AW；补 seam、孔洞、方向、容差、有限 pcurve/surface 组合的实际缺口；控制拓扑图所有权 | 构造→validity/自干涉诊断→STEP读回→Viewer；确认真实方向/面积/拓扑关系 |
| BY | 布尔、修复和历史结果组合满足后续编辑需要 | 复用 AJ/BG/既有 BooleanHistory；补批量输入、错误诊断、修复前后对应的缺口；不批量包装内部 BOPDS/TopOpe 存储 | Fuse/Cut/Common、失败、split/merge、删除和多路径；独立结果、过期状态拒绝；所有历史路径保留 |
| BZ | 局部特征与 sweep/offset 的较完整建模工作流 | 审核已实现 fillet/chamfer/offset/sweep，补选择/参数/诊断与结果追踪的缺口；私有 builder 成功后发布 | 边/面选择→建模→验证→保存重开→显示；错误参数、无解和回滚；阶段二完整消费者交付 |

顺序 BV→BW→BX→BY→BZ；如果特定 AppSweep 仍不安全，不影响使用既有安全路径的独立工作。
不能把缺失 history 用几何相似匹配代替。文件回读成功之外还需验证实际几何/拓扑。

BY 的固定源码审核与正式准入已完成，见 [布尔/修复执行清单](BATCH_BY_BOOLEAN_REPAIR_PLAN.md)：
21 个固定源文件、23 条 Manual 候选，已准入19条、另外4条继续待审；
BX 已冻结 PASS，BY 已完成实现和完整本地门禁，最终冻结后推进 BZ。

## 阶段三：网格、文档、装配与文件保真（CA–CE）

| 批次 | 较完整的目标 | 审计入口与规则工作 | 退出证据 |
|---|---|---|---|
| CA | 网格复制、受控重网格、质量检查与精确模型保持一致 | Poly_*、BRepMesh_* 消费者缺口；复用 AR/BE/BK/BO；审核 UV、法向、邻接、退化与写回映射 | 索引/方向/变换、退化/不完整网格、源释放；重网格保留源文档；proximity 不冒充精确碰撞 |
| CB | 直接属性、引用、撤销重做与持久化的一致性 | 复用 AS/AX/BI；TDataStd/TDF/TNaming 相关未覆盖合同；Bin/XML driver 依赖与 private persistence envelope | 创建/批改/失败回滚/undo/redo→XBF/XML重开；null slot/empty/cross-document拒绝与版本兼容 |
| CC | 多层共享装配和变换支持覆盖实际选定导出缺口 | 复用 BH/BJ/BP；审核 located definition vs occurrence、多个根、共享定义和标签身份，尤其 BP currently rejected 输入 | 明确可支持输入再实现；单位/累计变换、重复/循环、epoch过期、metadata保存；规范多文件重开 |
| CD | STEP 配置与 PMI/材质等高级属性形成明确的保真矩阵 | STEPCAF/STEPConstruct/XCAF DimTol/VisMaterial；AP242 及所需属性按 SDK/source/link 审核；已有 AP203/AP214 记复用 | exact supported fields 的导出/独立读回/差异报告，公开丢失/近似项；外部 CAD 证据缺失仍 NOT RUN |
| CE | IGES 与 glTF/OBJ/STL 等格式的可用导入导出链 | 从已有支持确认缺口；统一私有配置、单位/坐标、路径/纹理资源与复制诊断；多文件 IGES 先审计可行性 | 实际文件、Unicode/嵌套资源、格式能力差异、错误与回滚；源文件删除后独立资源；阶段三消费者交付 |

优先 CA→CB→CC，CD/CE 使用已完成的网格和文档合同。
AP242、PMI、PBR 与多文件 IGES 是待准入方向，不预先承诺全格式保真。
确定 SDK 不支持的路径要明确记录原因，不通过静默降级宣称支持。

## 阶段四：实际交互、应用与最终交付（CF–CI）

| 批次 | 较完整的目标 | 审计入口与规则工作 | 退出证据 |
|---|---|---|---|
| CF | Viewer 输入、选择、相机/裁剪及实际画面工作流 | 复用 parent-bound selection、ViewerInputController、AIS/V3d 合同；补确实缺失的输入/相机诊断 | 实际 HWND/WPF 交互、拾取身份、缩放旋转/裁剪和失效；客户端模型区域像素与UI事件路径证据 |
| CG | 多视口资源、材质与 render-health/recovery 的可靠生命周期 | 公共 Preview.67 与隔离 Cadoryx render-health.28 先做来源差异/兼容审核；审计 encoded texture、缓存模式、线程和 context 故障 | 创建/刷新/释放/源文件删除、其他视口不受扰动、actual context fault；物理 GPU removal 缺证据则单列 NOT RUN |
| CH | 真实 CAD 消费者的长流程与可重复性能验收 | 同一候选包贯穿读取→分析→编辑→历史→事务→导出→重开→显示；Cadoryx 等完整下游应用单独锁定版本验证 | 冷/暖与重复操作、峰值内存/持有资源、耗时分布、释放/取消行为；主机计时与物理显示/输入区分 |
| CI | 整轮冻结与可审查的本地交付候选 | exact delta、全局兼容性、源码与包身份、生成确定性、两个干净消费者及文档一致性收尾 | 全 Release/Debug、actual Debug、ABI/lifetime、cold regeneration、14包/消费者和release-check；18批结果与剩余门禁清单 |

CG 的私有 SDK 必须先兼容审核再合入公共基线；不能直接复用私有测试数宣称公共交付。
除已审计的 render-health.28 外，STATUS 中 SDK29 生成器纠正准备仅为待审核输入；
其实际生成/编译/运行/包门禁尚未通过，不能视为公共可复用交付。
CH 的应用验收独立于 facade/module 样例。CI 不包含未经授权的 push、签名或公开发布。
Hosted CI/外部 CAD/人工应用与设备验收按实际执行记录，通过前不得标为 PASS。

## 执行与交付规则

1. 开始每批时锁定上批已交付基线，生成 exact admitted/deferred/reused/prerequisite 清单，
   审核头文件、固定 SDK 实现、构造初始化、链接符号、ownership 和线程边界。
2. 改 emitter/type map/contract/template 后生成。Manual 例外写 SPECIAL_CASES；
   所有权变化写 OWNERSHIP，设计边界变化须先有接受的 ADR。
3. 开发中按 [WORKFLOW](WORKFLOW.md) 做影响范围检查；每个绑定/产品交付批次保留
   full Release/Debug、实际 Debug-native、冷生成、ABI/API/export/lifetime 与相应消费者门禁。
   所需门禁失败或 NOT RUN 时该批交付不能关闭，不扩范围逃避失败。
4. 每批分别记录 migrated IDs、Manual变化、新 discovery、复用与deferred；
   同一历史能力不重复计数。版本跟随有效交付，不预分配新版本或迁移配额。
5. BU/BZ/CE/CI 为阶段集成节点；不重复跑未变化且哈希身份一致的证据，
   新行为、新输入或源码变化必须获得对应的新证据。
6. 公开发布、签名、Hosted CI、完整下游与物理设备验收分别报告；
   本地PASS与生成成功均不能替代这些状态。

## 优先级、估算与剩余范围

BR–BY 交付后继续 BZ 的 Modeling/Geometry，再推进 CA–CI。
CA–CE 解决文件与文档保真，CF–CI 完成实际消费与交付闭环。
每批启动准入后才能估算工期和迁移数量；当前不提供虚假的完工日期。

18 批之外仍可能有内部算法、任意模板、借用/零复制、回调、平台适配、
可选 IVtk/Draw 与尚未发现的 SDK 声明。后续按消费者需求再决定，
不把 Skipped 或所有 Blocked 自动转成实现任务。
