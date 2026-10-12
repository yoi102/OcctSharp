# Preview.61 后下一轮规划：BL–BQ

日期：2026-10-11（Asia/Tokyo）。状态：**LOCAL GATES PASS; FINAL BQ FREEZE REQUIRED**。
这是本次进度复核提出的新六批规划；BF–BK 已冻结完成 6/6，原授权规划剩余 0。
用户已要求把六批规划全部做完；按 BL → BM → BN → BO → BP → BQ 连续实施及本地交付验证。具体版本随有效交付递增，不预分配版本或声明配额。

## 当前基线与范围

基线 Preview.61 / ABI 1.104 / bridge 0.112.0，交付权威为
`OcctSharp/artifacts/batch-bk/delivery-result.json` PASS。
[复核报告](PROGRESS_PREVIEW_61_REVIEW.md) 和
[统计输入](PROGRESS_PREVIEW_61_REVIEW_COUNTS.json) 已实时核对库存与冻结证据。
本次未重新运行构建、Runtime 或打包；复用身份一致的既有冻结结果。

已覆盖 28,503 个声明，非 Skipped 覆盖率 42.5678%；Blocked 38,456，
Skipped 49,407，解析排除 28。Modeling/DataExchange/Foundation 占 Blocked 的 69.07%。
本轮优先私有构造、复制输出和实际文件工作流，不迁移整个内部布尔/图存储实现，
不将所有 Blocked/Skipped 都当作承诺队列。

[候选输入](NEXT_ROUND_BL_BQ_CANDIDATES.json) 记录 399 个互不重复的 Blocked StableId，
逐项连接当前 inventory 的分类和 discovery 的签名、模块，并记录六个统计输入的哈希。
399 是家族审计输入，**不是准入数或必须新增的数量**；其中仍包含不适用的重载、
copy/move 构造、operator、借用访问器和内部算法辅助接口。
已有 Emitted/Manual 只标明历史覆盖，不自动获得本轮新增或已审核复用信用。
每批实施前分别形成 admitted / deferred / exact reused / necessary prerequisites。

固定 SDK 源码提交 `b8f597c677811d1f9f4d8a97f5ae2825c0353a42`。
本次通过 `git show` 初读了四个相关头文件，并在候选记录中保存原始字节哈希。
这不替代完整源码、安装头、链接符号、初始化和所有权准入审计：后者 **NOT RUN**。
不得用可变 SDK checkout 或只有头声明来证明可链接、可安全运行。

## 六批目标

| 批次 | 工作范围 | 原始 Blocked 家族输入 | 完整可验收结果 |
|---|---|---:|---|
| BL | 参数化边/面的私有构造 | 102 | 补 AP 之后的解析曲线、端点/顶点、pcurve/曲面和有限面域构造缺口；返回独立拓扑及原生错误诊断，实际组装和 STEP/IGES 回读。 |
| BM | 有限域极值和耦合复制输出 | 54 | 私有曲线/曲线、曲线/曲面、曲面/曲面计算，复制参数、点对、平方距离、平行/完成状态及适用的端点结果。 |
| BN | 有界字符串变换与批量复制规则 | 160 | 在 AV/SC-008 既有字符串能力上补独立变换、必要显式长度重载和批量复制；保持 UTF8/UTF16、索引和失败原子性。 |
| BO | 原生网格重叠和 proximity 诊断 | 51 | 基于已有三角网格复制域返回确定的重叠子形状配对、模式相关诊断和近似来源，不公开 BVH、mutable map 或 callback。 |
| BP | 选定标签/根和本地多文件装配导出 | 32 | 私有 STEPCAF writer 受控导出、文件清单/哈希和配置诊断；用 BJ 实际多文件导入回读，验证重复实例及格式支持的元数据。 |
| BQ | 整轮文档、文件、Viewer 和交付验收 | 0 固定新增 | 将新构造、极值/重叠、Unicode 文档和装配导出组合成两种公共消费者；实际保存重开、HWND/DXGI 身份和失效验证，冻结整轮交付。 |

BQ 是独立集成交付批次；其结果由实际工作流和完整门禁衡量，不为扩大数量添加无关接口。
各批围绕完整结果组织，不把单个 getter、测试或提交切成一个交付批次。
基础距离测量、已有网格编辑/邻接、基本 pipe/fillet/chamfer、已有字符串 split/substring
和默认交换均复用；不把它们作为新实现重复计算。

## BL：参数化构造

审计 BRepLib_MakeEdge、BRepBuilderAPI_MakeEdge、BRepBuilderAPI_MakeFace。
复用 AP 私有 builder、typed completed-result 和既有曲线/曲面/拓扑复制规则；
不同层级的等价重载不要求重复提供公共功能。

重点核验有实际消费者缺口的解析曲线区间、指定点/顶点端点、pcurve+surface，
以及平面/柱面/球面等有限 UV 面域。无限区间、默认未初始化结果和隐式转换不自动准入。
端点、曲线、曲面与输入顶点要在一致的复制图中使用；不能用坐标近似自动推断拓扑身份。
原生参数方向、周期接缝、退化边、错误状态与完成后拓扑分别表达。

验收包含正反参数方向、整周期/接缝、点不在曲线上、错误顶点、非法域、
有效 pcurve 回代、输入不被改变和输入释放后独立寿命。
将实际边/面组装为可查询拓扑，验证位置/边界以及 STEP/IGES 回读。
已有拓扑历史只在 SDK 明确产生且可证明时复用；不为构造重载发明 history。

## BM：有限域极值

审计 Extrema_ExtCC/ExtCS/ExtSS 和必要的 Extrema_POnCurv/POnSurf；
已有 GeomAPI_Extrema* 基本运算复用，借用 Extrema() 引用不能直接跨 ABI。
提供初始化完成的私有 adaptor/solver，显式有限参数域、容差与结果预算。

固定头文件说明 Initialize 不执行计算，单解配置只对参数求解器生效；
不能把配置请求宣称为全部解析路径都采用的结果性质。
点对、各侧参数与距离属于同一结果域；未完成、平行且无唯一点对、空结果分别表达。
TrimmedSquareDistances 的端点组合与内部驻点是不同类别。
保留 SDK 算法的数值/完备性边界，不把“找到若干极值”当成全局最小值证明。

验收覆盖解析对照、多解/并列、平行、端点最近、周期接缝、一般曲线/曲面、
非法容差、无结果、超限和释放后的独立数据；参数回代到原复制几何核对点对残差。
输出预算不等于可中断计算或严格超时。

## BN：字符串与复制规则

审计 TCollection_AsciiString/ExtendedString 的剩余重载，复用 AV 的私有字符串
初始化、UTF8/UTF16 和 SC-008 的既有复制能力。
优先有消费者需求的 Cat/AssignCat、字符替换、必要显式长度写入和 Unicode Token；
具体选项按当前未覆盖签名准入，避免重复已有插入/删除/split/substring 功能。

公共结果为独立值；私有修改只作用于本次副本。原字符串、string_view、
静态 EmptyString 借用引用、流或全局 locale 状态不直接逃逸。
延续已有非法编码、NUL 和最大字节数约束，UTF16 索引语义明确，禁止切断 surrogate pair。
批量输入必须 budget+1 有界枚举，检查全部操作与累计输出容量后执行；失败不发布部分结果。

验收包含 ASCII、中文、补充平面字符、空值/空集合、各侧界限、编码错误、
变换导致超限、惰性超限和输入释放。
实际将输出用于 BI 文档文本集合及 BP 的模型名称；文件名合法性与模型 Unicode 名称分别检查。
不通过修改全局类型映射把全部裸 char*/wchar_t* 自动变成字符串。

## BO：网格重叠与 proximity

主目标是 BRepExtrema_ShapeProximity；两个辅助家族仅作为审计来源，
BVH visitor/callback 和内部可变结构不因关联而自动准入。
复用 AR/BE/BK 的受控网格、实际三角网格深复制和精确来源索引；不重新实现已交付的编辑/邻接。

固定头文件区分三角网格相交、有限容差近邻及 proximity-value 模式。
后者是包容两形状的最小层厚度语义，不能标成精确 BRep 最短距离。
算法基于现有三角网格，结果是近似；网格来源、离散参数和可证明的误差边界单独说明。
先审计各模式的初始化字段，仅在对应模式成功后复制可用字段，其他字段明确缺失。

Overlap/GetSubShape 的原生索引从 0 开始，与 XDE 的 1-based 查询不同。
配对和子形状必须属于该次输入复制域；排序/去重不能改变 SDK 报告的关系。
限定 faces/triangles/samples、配对数量和输出存储；不把后验数量检查当成原生计算超时。

验收包含分离、交叉、有限容差内外、不同网格密度、重复/退化输入、缺失网格、
各模式状态、超限及输入关闭后独立寿命。
用既有精确距离/布尔能力作明确有限的参考比较，不能据少量样本宣称全局等价。

## BP：装配导出

审计 STEPCAFControl_Writer、IGESCAFControl_Writer；复用 AX/BD/BJ/BK 的
私有模型参数、文档/context/label 身份、文件清单和 staging 机制。
重点补指定标签序列和本地多文件 STEP 输出，保持配置与进程全局设置隔离。

固定头文件提供多文件 Transfer 和 ExternFiles 路径；完整实现前必须审计
外部文件命名、prefix、重复名称、writer 修改行为和实际 Write 路径。
在调用 SDK 前证明所有写入目标都位于私有 staging 域内；模型名称和文件名分开处理。
仅对完整输出集发布清单与哈希，失败清理本次创建的文件，拒绝覆盖已有目标。
若无法证明写入范围或保真核心目标，保持该目标阻塞；不能只在写后检查路径，
也不能用单文件旧 API 代替多文件交付目标。

验收包括单根/选定多个标签、外域标签拒绝、重复实例、同名部件、文件名碰撞、
失败清理、缺失输出、文件改写和模型私有配置隔离。
通过 BJ 实际多文件 reader 回读新输出，核对文件依赖和精确 occurrence/definition 关系。
STEP/IGES 名称、颜色、层等按各格式实际支持范围验证；不承诺通用 PBR/PMI 全保真。
外部 CAD 互操作仍是独立验收，不用同 SDK 回读替代。

## BQ：真实集成与冻结

两个干净消费者分别执行 BL 几何构造、BM 极值、BO 网格诊断、
BN 文档文本、BP 实际导出/回读和 XBF 保存重开。
在真实 HWND 中将新回读实例注册为 presentation，比较 selected/picked 身份，
记录可见 DXGI 像素，并验证 visibility/transform 改变后的旧查询失效。
保留已通过的旧工作流；新链必须使用本轮实际生成的文件和源身份，不能引用旧截图代替。

完善各批当前状态与局限，核对包内文档、源输入、managed/native DLL、SBOM、
provenance/checksum 和冻结证据。BQ 不包含未授权的提交、push、签名或公开发布。

## 执行顺序、交付与剩余项

建议 BL → BM → BN → BO → BP → BQ。
BL/BM/BN 为几何、复制结果和名称提供前置；BO 复用已有网格能力，BP 复用 BN 与 BJ，
BQ 组合整轮。开发期按 [WORKFLOW](WORKFLOW.md) 选择影响相关检查，
每批产品交付保留完整 Release/Debug、实际 Debug-native、冷源码/确定性、
ABI/API/导出、寿命/并发、精确库存差量、14 包、两个消费者和统一 release-check。
仅本批 `delivery-result.json` PASS 才计完成；有实现但门禁未通过仍是未交付。

若准入暂缓影响核心目标，不能靠少量无关声明增加获得该批完成；必须修复原因或明确调整目标。
28 个解析排除继续单列，在必要前置审计中按实际发现修复；不保证清零，也不猜未发现数量。
不扩大到全部内部算法、任意 callback/zero-copy、平台扩展、DLL 拆分、
全局可变设置或不可靠的拓扑自动重绑定。GeomFill_AppSweep 固定 SDK 缺陷继续暂缓。

六批已获连续实施授权；BL–BP 已冻结 PASS，BQ 整轮集成及本地门禁通过，其最终 batch-bq/delivery-result.json PASS 关闭 6/6，规划剩余 0；这并非完整 SDK 迁移完成。完整迁移的总批数与时间目前不可可靠估算。
Hosted CI、人审/完整下游验收、外部 CAD 互操作、签名、push 和公开发布分别保留 NOT RUN。
本次文档是冻结 Preview.61 之后的源文件变更，不属于已验证的旧包内容；
下次交付时再打包核对，不修改六批旧交付记录。
