# Preview.67 整体复核与代码来源说明

日期：2026-10-11（Asia/Tokyo）。本轮为总结与规划，未实施新的绑定或发布。
基线：8.0.1-preview.67 / ABI 1.110 / bridge 0.118.0。

## 代码是否全部由生成器生成

**整个仓库不是全部自动生成；最近 BL–BP 新迁移的 91 个声明均由生成器输出，Manual 增量为 0。**

| 层次 | 当前来源与责任 | 具体位置（仓库相对路径） |
|---|---|---|
| 发现、绑定模型、分类与类型映射 | 人工维护的生成器程序读取 C++ 声明，选择可安全暴露的接口 | `OcctSharp/src/OcctSharp.Generator/Discovery/`、`Transformation/`、`TypeMapping/` |
| 通用与专用生成逻辑 | 人工编写 emitter、JSON 合同和 native/managed 文本模板；安全语义不会仅靠头文件自动推导 | `OcctSharp/src/OcctSharp.Generator/Emission/`；项目文件将合同与模板嵌入资源 |
| 生成的 native 桥与 C# 绑定 | 生成器输出，应从规则或模板修复后再生成 | `OcctSharp/src/OcctSharp.Native/generated/`、`OcctSharp/src/OcctSharp/Generated/` |
| 手写门面、人工绑定与基础设施 | ShapeFactory、ShapeExchange、OcctViewer、XdeDocument、生命周期、错误与运行时等仍有人工作业 | `OcctSharp/src/OcctSharp/` 非 Generated 部分，以及 `OcctSharp/src/OcctSharp.Native/src/` |
| 测试、样例、交付工具 | 人工维护，用于证明生成结果与完整消费者工作流；BQ 没有新增迁移声明 | `OcctSharp/tests/`、`OcctSharp/samples/`、`OcctSharp/eng/` |

本次实时检查 manifest：27,367 个 sourceStableIds；309 个文件，包含
122 个 C#、90 个 C++、85 个 `.h`、12 个 `.hxx`。所有 309 个当前文件的
SHA-256 与 manifest 一致。文件数、行数及模板体积不能换算为产品能力完成度。
库存还有 1,227 个 Manual 声明；这个数也不是所有手写文件或辅助函数的数量。

BP 的私有 STEPCAF writer 来自固定 SDK 源码的生成器转换，保留源码哈希和版权。
它不是完全从声明自动推导，也不能把复制的 SDK 实现方法全部计为新增迁移。
现有人工例外见 [SPECIAL_CASES](SPECIAL_CASES.md)，所有权合同见 [OWNERSHIP](OWNERSHIP.md)。

## 整体进度及统计口径

| 分类 | 数量 | 含义 |
|---|---:|---|
| Emitted | 27,367 | 生成器已输出的声明 |
| Manual | 1,227 | 人工绑定分类声明 |
| 已覆盖合计 | 28,594 | Emitted + Manual |
| Blocked | 38,365 | 尚不满足绑定条件或有意暂缓的声明；需按消费者需求准入 |
| Skipped | 49,407 | 跳过的 operator、类型元数据、非公开项、测试工具等；不是待实现清单 |
| Pending | 0 | 已发现库存分类闭合，不能说明全 SDK 已迁移 |
| 总库存 | 116,366 | 已发现的声明；失败头文件内的未知声明未计入 |

非 Skipped 分母为 66,959，声明覆盖率 **42.7037%**；差额为 38,365，
占该分母 57.2963%。全库存已覆盖占比为 24.5725%。这些均是声明口径，
**不能称为 CAD 产品完成 42.7%，也不能估算整个 SDK 还差多少固定批次。**
当前扫描 7,062 / 7,090 头文件，28 个失败；完整语义扫描仍不完整。
配置 excludedHeaders 数量与扫描失败数不是同一个口径，应读取实际 Failures。

| 模块 | Emitted + Manual | Blocked | 非 Skipped 声明覆盖 | 下一步重点 |
|---|---:|---:|---:|---|
| Foundation | 3,732 | 7,163 | 34.25% | 复制容器、值类型、单位与消息诊断的完整生命周期 |
| Geometry | 6,452 | 5,308 | 54.86% | 样条/拟合、投影与交点的边界、复杂工作区 |
| Modeling | 3,649 | 11,381 | 24.28% | 精确拓扑构造、布尔/修复、局部特征及可靠 history |
| MeshData | 175 | 124 | 58.53% | 节点/三角形/UV/邻接复制和写回一致性 |
| Mesh | 183 | 307 | 37.35% | 受控重网格、质量及退化诊断 |
| Documents | 1,666 | 2,272 | 42.31% | 属性/引用、事务、撤销重做与持久化保真 |
| DataExchange | 7,715 | 7,961 | 49.22% | 格式配置、文件链、元数据及诊断保真 |
| Xde | 769 | 599 | 56.21% | 装配定义/实例变换、PMI与交换的明确支持范围 |
| Visualization | 4,253 | 3,231 | 56.83% | 实际交互、资源恢复、材质与多视口生命周期 |
| IVtk | 0 | 0 | 不适用 | 可选 VTK 依赖，暂不列入核心轮次 |
| Draw | 0 | 19 | 0% | 主要为 Draw/测试工具，暂不列入核心轮次 |

主要阻塞原因：非 Transient 接收者所有权 14,438、值构造 5,465、
未映射值类型 5,113、借用/输出引用 4,973、裸指针生命周期 3,547、
未选择 Handle 目标 2,530。需要解决共享的生成规则与完整所有权合同，
而不是逐条机械添加包装。Modeling/DataExchange/Foundation 的 Blocked
合计 26,505，约占 69.09%，优先服务这些模块的实际 CAD 缺口。

## 已完成的最新六批

| 批次 | 完整工作流 | 版本 | 新增 generated 声明 |
|---|---|---|---:|
| BL | 有限参数化边/面私有构造 | Preview.62 | 17 |
| BM | 有限域极值和耦合复制输出 | Preview.63 | 23 |
| BN | 有界字符串变换、批量复制及原子失败 | Preview.64 | 14 |
| BO | 独立面网格 overlap/proximity 诊断 | Preview.65 | 22 |
| BP | 选定标签/根的多文件 STEP 装配导出 | Preview.66 | 15 |
| BQ | 整轮实际文件、XBF、Viewer与两个消费者交付 | Preview.67 | 0 |
| 合计 | 六份 delivery-result.json 均冻结 PASS | 6/6，剩余 0 | 91 |

已经具备多条建模、几何分析、复制容器、修复、装配、文档、网格、
STEP/IGES 与 Viewer 工作流。后续规划应复用其已验收能力，补未覆盖的输入、
结果与实际消费者路径；不能把历史已有工作重新算作新增迁移。

## 已验证、尚未验证与明确限制

冻结证据记录：Release/Debug/cold Generator 各 313、Runtime 各 1,349；
实际 Debug-native 1,349 和 62 DLL 实载路径/哈希；309 个生成文件确定性；
41,460 exports、58,819 API signatures、4,437 facade forwarders；
14 本地包、13 managed payload、两个干净消费者及 ABI/生命周期/并发释放门禁通过。
BQ 的 facade/module/actual-debug/final 四上下文使用新 STEP 文件、实际 XBF
值和真实 HWND/DXGI 图片，验证身份与 visibility/transform stale 行为。

本轮只实时核对库存、manifest 所有文件和六份冻结 authority 的链式哈希；
**没有重跑这些构建/测试，也没有重新审计全部冻结包和源快照。**
复核记录见 [REVIEW_COUNTS](PROGRESS_PREVIEW_67_REVIEW_COUNTS.json)。
旧 PROGRESS_PREVIEW_67_COUNTS.json 的 frozenDeliveryVerified=false 是统计报告生成时
的状态；交付完成由后来的 delivery-result.json 与 authorized-round-result.json 决定。

仍有明确限制：

- GeomFill_AppSweep 的固定 SDK 缺陷仍暂缓；需复现、修复或明确替代合同才能准入。
- topology history 在多路径、split/merge 或证据不足时不能猜测或自动重绑定。
- BM 有限域极值不承诺全局最小或算法完备；BO 是近似网格诊断，不提供认证误差或执行超时。
- BP 当前仅 AP203/AP214、mm、明确 names/colors/layers；located assembly definitions 拒绝。
  PBR/PMI 全面保真、多文件 IGES、任意深度/尺寸装配均未承诺。
- Cadoryx 私有 render-health.28 单独推进，尚未集成公共 Preview.67；其 1,358 测试
  不能混入公共基线 1,349。故障注入、物理 GPU 移除及完整下游恢复仍缺证据。
- Hosted CI、完整下游应用验收、外部 CAD 互操作、签名、push、公开 NuGet：**NOT RUN**。

## 下一轮

[BR–CI 长期规划](NEXT_ROUND_BR_CI_PLAN.md) 提出 18 个工作流批次，分四阶段。
当前 **PROPOSED / NOT STARTED，0/18**；本次请求只总结与规划。
每批先做 exact admission；对已有能力记 reused，对不安全/无消费需求项记 deferred。
各批完成适用的交付门禁再扩范围，不预定版本、迁移数量或全 SDK 完成百分比。
本轮新文档不属于历史 Preview.67 冻结包；不重新封装历史版本。
