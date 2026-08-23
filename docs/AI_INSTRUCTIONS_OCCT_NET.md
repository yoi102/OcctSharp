# AI_INSTRUCTIONS.md

> 当前仓库说明（2026-08-21）：本文件保留为长篇背景规则。自动化 Agent 的入口是
> 根目录 `AGENTS.md`；当前架构、目录和进度以 `docs/DOCUMENTATION_INDEX.md` 索引的专题文档、
> 已接受 ADR 及 `docs/STATUS.md` 为准。本文件中把 `src/`、`tests/`、`reports/`
> 放在仓库根目录的旧目录示例已由 `docs/REPOSITORY_LAYOUT.md` 取代。
>
> 本文件用于约束所有参与本项目的 AI Agent（如 ChatGPT、Codex、Claude Code、Devin 等）。
>
> 目标：构建一个可维护、可升级、可验证的 **Open CASCADE Technology（OCCT）C#/.NET 自动封装生成器与运行时封装库**。
>
> 本文件优先级高于 AI 自行推测的实现偏好。除非用户明确要求，否则不得违反本文件中的规则。

---

# 1. 项目目标

本项目不是简单地把 OCCT 的 C++ API 全部机械翻译成 C#。

项目目标是：

1. 使用 Clang/Clang AST 或等价可靠的 C++ AST 解析方式读取 OCCT Headers。
2. 自动生成底层 Native Binding。
3. 自动生成对应的 C# Wrapper。
4. 对 OCCT 特殊类型建立稳定的 TypeMap。
5. 明确定义 Native/C# 两侧的对象所有权和生命周期。
6. 自动统计 API Binding Coverage。
7. 自动执行编译、单元测试、集成测试和 Runtime Validation。
8. OCCT 升级版本后，应尽可能通过重新生成而复用现有封装体系。
9. Generated 代码应尽可能由生成器产生，而不是人工维护。
10. 对无法自动封装的特殊类型允许使用 Manual Wrapper，但必须记录原因。

推荐总体结构：

```text
OCCT Headers / Libraries
          ↓
      Clang AST
          ↓
   Binding Generator
          ↓
 ┌───────────────────┐
 │ Native Generated  │
 │ C# Generated      │
 └───────────────────┘
          ↓
      Manual Layer
          ↓
       Public API
```

---

# 2. AI 工作基本原则

AI 在修改本项目时必须遵循以下原则。

## 2.1 不允许无上下文直接修改

开始任何较大任务前，必须优先阅读：

```text
docs/STATUS.md
docs/ARCHITECTURE.md
docs/TYPE_MAPPING.md
docs/OWNERSHIP.md
docs/SPECIAL_CASES.md
docs/KNOWN_ISSUES.md
docs/COMPATIBILITY.md
docs/DECISIONS.md
```

如果文件尚不存在，应根据本文件建立。

AI 不得因为上下文窗口中没有看到旧设计，就自行重新设计同一机制。

---

## 2.2 不允许为了“完成任务”破坏已有架构

禁止：

- 为了让测试通过而绕开既有 Ownership 体系。
- 同一种 Native 类型在不同位置采用不同封装方式。
- 同一种 `Handle<T>` 出现多套生命周期模型。
- 在 Generated 文件中直接打补丁作为长期方案。
- 为某一个 API 添加一次性 hack，而不考虑生成器规则。
- 为了提高覆盖率而生成明显不安全或不可用的 API。
- 为了降低失败数量而隐藏、删除或忽略失败统计。

所有特殊处理必须能够解释：

```text
为什么需要？
影响哪些类型？
是否可以泛化？
是否应该进入 Generator Rule / TypeMap？
```

---

# 3. Generated 与 Manual 的边界

目录应清楚区分：

```text
Generated/
Manual/
```

规则：

## Generated

由代码生成器产生。

禁止人工直接修改。

如果 Generated 代码错误，应优先修改：

```text
Parser
AST Model
TypeMap
Pass
Emitter
Generator Rule
```

然后重新生成。

## Manual

只允许用于：

- OCCT 特殊生命周期类型。
- 无法可靠自动绑定的复杂模板。
- 高层 .NET Friendly API。
- Native ABI 边界辅助代码。
- 特殊异常处理。
- 特殊容器转换。
- 特殊性能优化。

任何 Manual Wrapper 都必须在：

```text
docs/SPECIAL_CASES.md
```

中记录。

---

# 4. C++ 解析规则

禁止使用 Regex 作为主要 C++ Parser。

禁止以如下方式实现主解析器：

```text
Regex("class ...")
字符串切割 Header
简单括号匹配
```

必须优先使用：

- Clang AST
- libclang
- ClangSharp
- CppSharp
- 或其他完整 C++ AST Parser

原因：

OCCT Header 中可能存在：

- namespace
- template
- specialization
- typedef
- using
- macro
- inheritance
- multiple inheritance
- const
- reference
- pointer
- overload
- operator
- default parameter
- nested type

Regex 只能作为辅助文本处理工具，不能承担语义解析职责。

---

# 5. Type Mapping 规则

所有 Native → Managed 映射必须集中管理。

建议文件：

```text
docs/TYPE_MAPPING.md
```

示例：

```text
Standard_Integer
→ System.Int32

Standard_Real
→ System.Double

Standard_Boolean
→ System.Boolean

Standard_CString
→ System.String

TCollection_AsciiString
→ System.String 或专用 Wrapper

gp_Pnt
→ Point3d / GpPnt

gp_Pnt2d
→ Point2d / GpPnt2d

gp_Vec
→ Vector3d / GpVec

TopoDS_Shape
→ Shape

TopoDS_Face
→ Face

TopoDS_Edge
→ Edge

TopoDS_Vertex
→ Vertex

Handle<T>
→ Managed Reference Wrapper
```

任何新增 TypeMap：

1. 必须有唯一规则。
2. 必须有测试。
3. 必须更新 `TYPE_MAPPING.md`。
4. 必须考虑 const/reference/pointer/value 的区别。
5. 必须考虑生命周期。
6. 不允许同一个 Native 类型在不同模块映射成不同 Managed 类型，除非有明确设计说明。

---

# 6. Ownership / Lifetime 是最高优先级规则

OCCT Wrapper 中，生命周期错误比 API 缺失更加严重。

所有对象必须明确：

```text
Who creates it?
Who owns it?
Who releases it?
Is it borrowed?
Is it copied?
Is it reference-counted?
Can it outlive its parent?
```

必须维护：

```text
docs/OWNERSHIP.md
```

建议给 Ownership Rule 编号：

```text
O001
O002
O003
...
```

例如：

```text
O001:
任何跨 Native ABI 返回给 C# 的 owning object
必须存在明确释放函数。

O002:
Borrowed pointer 不得被 Managed Wrapper 误释放。

O003:
Handle<T> 必须保持 OCCT intrusive reference counting 语义。

O004:
const T& 不允许在无法确认生命周期时直接暴露为长期 Managed pointer。

O005:
Dispose 必须支持重复调用而不导致 double free。

O006:
Finalizer 只能作为兜底，不应代替确定性 Dispose。

O007:
Managed object Dispose 后再次访问 Native Handle 必须失败或安全返回，
不得产生 use-after-free。

O008:
任何 Native exception 不得直接穿越 C ABI。
```

AI 不得自行改变已有 Ownership Rule。

如果必须改变：

1. 先记录 Decision。
2. 说明旧规则。
3. 说明新规则。
4. 说明迁移影响。
5. 更新对应测试。

---

# 7. Handle<T> 特殊规则

OCCT 的：

```cpp
Handle(SomeType)
```

不是普通裸指针。

必须视为 OCCT 引用计数对象。

生成器必须能够识别：

```text
Handle<T>
opencascade::handle<T>
Standard_Transient 派生类
```

必须验证：

- 创建。
- Copy。
- Base → Derived。
- Derived → Base。
- Dispose。
- 多个 Managed Wrapper 引用同一 Native Object。
- Parent Dispose 后 Child 是否仍有效。
- GC 后引用计数是否正确。
- 异常路径是否泄漏引用。

不得简单统一映射为裸 `IntPtr` 后忽略引用计数语义。

---

# 8. TopoDS_* 特殊规则

以下类型不得默认视为普通 heap pointer：

```text
TopoDS_Shape
TopoDS_Face
TopoDS_Edge
TopoDS_Vertex
TopoDS_Wire
TopoDS_Solid
TopoDS_Shell
TopoDS_Compound
TopoDS_CompSolid
```

必须单独验证：

- Copy 行为。
- `IsNull()`。
- `Orientation`。
- `Location`。
- `TShape` 共享关系。
- Shape subtype 转换。
- 生命周期。
- Hash / equality 语义。

相关设计必须记录在：

```text
docs/SPECIAL_CASES.md
```

---

# 9. Container / Template 规则

OCCT 可能包含：

```text
NCollection_List<T>
NCollection_Vector<T>
NCollection_Sequence<T>
NCollection_Map<T>
NCollection_DataMap<K,V>
NCollection_Array1<T>
NCollection_Array2<T>
```

禁止简单假设所有模板都可以自动映射。

每个 Container TypeMap 必须明确：

```text
Managed representation
Ownership
Iteration
Copy semantics
Mutation semantics
Index base
Lifetime
```

优先提供 .NET Friendly API，例如：

```text
IReadOnlyList<T>
IEnumerable<T>
Dictionary<K,V>
```

但不得因为“更像 C#”而改变 OCCT 原有语义。

---

# 10. Exception Boundary

禁止 C++ Exception 穿越 Native ABI。

必须在 Native Bridge 捕获至少：

```text
Standard_Failure
std::exception
unknown exception
```

然后转换为统一错误模型。

建议：

```text
Native Error
    ↓
Error Code + Error Message
    ↓
OcctException
```

必须测试：

- 正常调用。
- `Standard_Failure`。
- invalid argument。
- null native handle。
- unexpected exception。

禁止吞掉异常后静默返回默认值。

---

# 11. API Binding Coverage

普通代码 Coverage 不能代替 Binding Coverage。

必须统计至少：

```text
Headers discovered
Namespaces discovered
Classes discovered
Classes generated
Classes skipped

Methods discovered
Methods generated
Methods skipped

Constructors discovered
Constructors generated

Enums discovered
Enums generated

Functions discovered
Functions generated

Properties/getters/setters

Handle<T> types

Templates
```

建议输出：

```text
reports/binding-report.json
reports/binding-report.md
```

例如：

```text
Classes:
  discovered: 12430
  generated:   9212
  coverage:    74.1%

Methods:
  discovered: 126403
  generated:   94822
  coverage:    75.0%
```

不得只输出一个总百分比。

---

# 12. Module Coverage

必须尽量按 OCCT 模块/Toolkit/Package 统计覆盖率。

例如：

```text
TKernel
TKMath
TKG2d
TKG3d
TKGeomBase
TKBRep
TKTopAlgo
TKBO
TKSTEP
TKSTEPBase
TKSTEPAttr
TKMesh
TKV3d
```

报告至少包括：

```text
Module
Classes discovered
Classes generated
Methods discovered
Methods generated
Coverage
Unsupported count
```

这样可以明确当前完成到哪个模块。

---

# 13. Skip 必须有原因

任何没有生成的 API 必须有 Skip Reason。

禁止：

```text
Skipped: 35100
```

必须分类，例如：

```text
Unsupported template
Unknown TypeMap
Private/protected API
Operator overload
Variadic function
Function pointer
Multiple inheritance
Macro-generated declaration
Unsupported ownership
Manual blacklist
Deprecated API
Parser failure
Generator failure
Compile failure
```

必须统计各原因数量。

如果某个 Skip Reason 数量很大，AI 应优先判断是否存在可泛化解决方案。

---

# 14. 测试分层

必须区分至少以下测试。

## 14.1 Generator Unit Tests

验证：

- AST parsing。
- TypeMap。
- Rename。
- Ignore Rule。
- Constructor generation。
- Method generation。
- Overload。
- const/reference/pointer。
- Enum。
- Handle。
- Container。
- Error generation。

## 14.2 Generated Code Compile Test

生成后的代码必须实际编译。

禁止只测试字符串输出是否“看起来正确”。

必须至少验证：

```text
Native Generated Code Compile
Managed Generated Code Compile
```

## 14.3 Runtime Binding Test

实际调用 OCCT。

例如：

```text
Create gp_Pnt
Read coordinates
Create TopoDS_Shape
Copy Shape
Dispose Shape
Call geometry operation
```

## 14.4 Integration Test

测试完整链路，例如：

```text
C#
↓
Generated Managed Wrapper
↓
Native Bridge
↓
OCCT
↓
Return Result
```

## 14.5 Real File Test

必须逐渐加入真实文件：

```text
STEP
IGES
BREP
OBJ
STL
```

至少对核心目标模块使用真实 CAD 文件验证。

---

# 15. 测试指标必须分开记录

不得用单一 Coverage 数字代替全部质量指标。

至少区分：

```text
Generator Line Coverage
Generator Branch Coverage

Binding API Coverage

Compile Validation Coverage

Runtime Validation Coverage

Integration Validation Coverage

Real File Validation Coverage
```

例如：

```text
Generator:
  Line: 91.4%
  Branch: 84.2%

Binding:
  Class: 74.1%
  Method: 75.0%

Validation:
  Compile: 100%
  Runtime: 13.2%
  Integration: 5.1%
```

---

# 16. Runtime Validation 比“生成成功”更重要

必须牢记：

```text
能解析
≠
能生成

能生成
≠
能编译

能编译
≠
能调用

能调用
≠
生命周期正确

生命周期正确
≠
真实项目可用
```

因此报告中必须明确区分每一阶段。

---

# 17. 必须维护 STATUS.md

建议：

```text
docs/STATUS.md
```

内容至少包括：

```text
OCCT Version
Generator Version
Current Phase
Current Focus
Last Completed
Current Blockers
Next Tasks
Known Risk
Do Not Change
Last Validation Result
```

模板：

```markdown
# Current Status

## OCCT Version

7.x.x

## Generator Version

0.x.x

## Current Focus

...

## Last Completed

- ...

## Current Blockers

- ...

## Next Tasks

1. ...
2. ...
3. ...

## Do Not Change

- Ownership Rule O001-O00X
- Exception boundary
- Generated files manually
- ...

## Last Validation

Native Compile: PASS
Managed Compile: PASS
Unit Tests: PASS
Integration Tests: ...
```

每次有实质进展后更新。

---

# 18. 必须维护 KNOWN_ISSUES.md

格式建议：

```markdown
## KI-001

Status:
Open

Severity:
High

Area:
Handle<T>

Problem:
...

Reproduction:
...

Expected:
...

Current workaround:
...

Planned fix:
...
```

Issue 被解决后不得直接删除。

应修改为：

```text
Resolved
```

并记录解决方式。

---

# 19. 必须维护 Architecture Decision

对于重要设计变更，必须记录 Decision。

建议：

```text
docs/DECISIONS.md
```

或者：

```text
docs/adr/
0001-native-abi.md
0002-handle-lifetime.md
0003-topods-shape.md
```

Decision 至少包含：

```text
Context
Decision
Reason
Alternatives
Rejected alternatives
Impact
Migration
Tests
```

AI 不允许只写：

```text
Changed Handle implementation.
```

必须说明为什么。

---

# 20. 兼容性记录

维护：

```text
docs/COMPATIBILITY.md
```

至少记录：

```text
OCCT Version
Compiler
Platform
Architecture
.NET Version
Generator Version
Status
Known breaking changes
```

例如：

```text
OCCT 7.9.x
MSVC 2022
Windows x64
.NET 10
PASS
```

当 OCCT 升级时：

1. 不允许直接假设兼容。
2. 必须重新解析。
3. 重新生成。
4. 对比 API Diff。
5. 编译。
6. 跑完整测试。
7. 输出升级报告。

---

# 21. OCCT 升级报告

版本升级后建议自动生成：

```text
reports/upgrade-7.9-to-8.0.md
```

至少包括：

```text
Added classes
Removed classes
Changed methods
Changed constructors
Changed inheritance
Changed enums
Changed templates
Changed Handle relationships
TypeMap failures
Compile failures
Runtime regressions
```

---

# 22. Public API 与 Raw Binding 分离

推荐至少区分：

```text
Raw Binding
Friendly Managed API
```

例如 Raw Binding 可以接近：

```text
BRepAlgoAPI_Fuse
TopoDS_Shape
STEPControl_Reader
```

高层 API 可以提供：

```text
StepDocument
Shape
Face
Edge
BooleanOperations
StepReader
StepWriter
Mesher
Viewer
```

禁止为了让 Public API 好看而破坏 Raw Binding 的语义。

也禁止把所有 OCCT 内部复杂度原样暴露给普通 C# 用户。

---

# 23. API 命名规则

必须统一命名。

例如需要明确：

```text
TopoDS_Shape → Shape
TopoDS_Face → Face
TopoDS_Edge → Edge
```

或者保留 OCCT 名：

```text
TopoDSShape
TopoDSFace
TopoDSEdge
```

二者必须二选一并保持一致。

禁止不同模块出现：

```text
Shape
TopoDSShape
TopoDsShape
OcctShape
```

除非它们确实是不同层的 API。

---

# 24. Native ABI 规则

如果项目采用 C ABI + P/Invoke：

必须：

- 使用稳定导出命名。
- 明确 calling convention。
- 明确 bool 大小。
- 明确 string encoding。
- 明确 struct layout。
- 明确 enum size。
- 明确 ownership。
- 明确 error handling。
- 避免直接跨 ABI 暴露 STL 类型。
- 避免直接跨 ABI 暴露 C++ exception。
- 避免 ABI 依赖 C++ name mangling。

禁止直接将：

```text
std::string
std::vector<T>
std::shared_ptr<T>
C++ class object layout
```

作为不稳定 ABI 暴露给 C#。

---

# 25. String 规则

所有字符串必须明确：

```text
Encoding
Ownership
Nullability
Lifetime
```

禁止直接返回 Native `char*` 给 C# 后依赖不明确生命周期。

推荐 Native → Managed 时复制。

必须处理：

```text
Standard_CString
TCollection_AsciiString
TCollection_ExtendedString
UTF-8
UTF-16
```

并通过测试验证非 ASCII 内容。

---

# 26. Null 规则

所有可以为空的 Native Object 必须明确：

```text
null managed reference
IntPtr.Zero
Null Native Handle
Null TopoDS_Shape
```

它们不是天然等价。

必须定义统一策略。

---

# 27. Dispose 规则

所有拥有 Native Resource 的 Managed Object：

```text
IDisposable
```

必须支持：

```text
using
Dispose twice
GC fallback
Access after dispose
Exception during disposal
```

测试必须覆盖。

---

# 28. 多线程规则

不得默认所有 OCCT 对象都是线程安全的。

如果没有明确证据：

```text
不要自动标记为 thread-safe。
```

任何：

```text
static global state
Interface_Static
Viewer context
Document
Allocator
```

等潜在线程安全问题必须记录。

---

# 29. 性能规则

不允许为了 API 优雅而引入明显高频额外开销。

尤其注意：

```text
每个坐标 getter 都跨 Native ABI
大量小对象不断 new/dispose
Iterator 每一步跨 P/Invoke
大量 string copy
大量临时 Handle wrapper
```

如果发现热点：

1. 先 benchmark。
2. 再优化。
3. 记录 Decision。

禁止没有 benchmark 就进行大规模“性能优化”。

---

# 30. Benchmark

建议对核心场景维护 Benchmark：

```text
Read STEP
Write STEP
Enumerate Faces
Enumerate Edges
Mesh Shape
Boolean Fuse
Transform Shape
Projection
Viewer object creation
```

比较至少：

```text
Native OCCT baseline
Managed Wrapper
```

以判断 Wrapper overhead。

---

# 31. 真实业务优先级

不要为了追求 100% OCCT API Coverage 而阻塞核心功能。

推荐优先级：

```text
1. Foundation / Standard
2. gp
3. TopoDS
4. TopExp
5. BRep
6. BRepAdaptor
7. Geom / Geom2d
8. BRepAlgoAPI
9. STEP
10. IGES
11. Mesh
12. Visualization / AIS
13. XDE / OCAF
14. Less-used modules
```

具体顺序可以根据用户需求调整。

---

# 32. AI 每次任务开始前必须执行的检查

开始修改前：

```text
[ ] 阅读 STATUS.md
[ ] 阅读 ARCHITECTURE.md
[ ] 阅读 TYPE_MAPPING.md
[ ] 阅读 OWNERSHIP.md
[ ] 阅读 SPECIAL_CASES.md
[ ] 阅读 KNOWN_ISSUES.md
[ ] 阅读最近 Binding Report
[ ] 确认本次修改属于 Generated 规则还是 Manual 特例
[ ] 确认不会破坏既有 Ownership Rule
[ ] 确认当前 OCCT 版本
```

---

# 33. AI 每次任务结束前必须执行的检查

完成修改后：

```text
[ ] 运行 Generator
[ ] Native Generated Code 编译
[ ] Managed Generated Code 编译
[ ] Unit Tests
[ ] Runtime Binding Tests
[ ] Integration Tests
[ ] 更新 Binding Coverage
[ ] 更新 Skip Reasons
[ ] 更新 STATUS.md
[ ] 新 TypeMap → 更新 TYPE_MAPPING.md
[ ] 新 Ownership Rule → 更新 OWNERSHIP.md
[ ] 新 Special Case → 更新 SPECIAL_CASES.md
[ ] 新 Known Issue → 更新 KNOWN_ISSUES.md
[ ] 新重要设计 → 更新 DECISIONS/ADR
[ ] 确认 Generated/ 中没有人工直接修补
```

如果某一步无法执行，必须明确记录：

```text
NOT RUN
```

以及原因。

禁止把“未执行”报告成“PASS”。

---

# 34. AI 输出进度时必须使用事实状态

AI 不得使用模糊表述：

```text
应该完成了
看起来可以
大概支持
基本没问题
```

应使用：

```text
Implemented
Generated
Compiled
Tested
Runtime Validated
Not Tested
Blocked
Unsupported
```

例如：

```text
Handle<T> parsing:
Implemented

Generated compile:
PASS

Runtime ownership test:
NOT RUN

Derived Handle cast:
BLOCKED
```

---

# 35. 禁止伪造完成度

AI 不得：

- 没跑测试却写 PASS。
- 没统计 API 就估算 Coverage。
- 没读取 Header 就声称模块完全支持。
- 因为代码能编译就说 Runtime 可用。
- 因为一个 STEP 文件成功就说 STEP 完全支持。
- 删除失败测试来提高通过率。
- 忽略 unsupported API 来提高覆盖率。
- 通过缩小统计分母制造更高 Coverage。

---

# 36. Coverage 分母必须稳定

API Coverage 的分母必须来自：

```text
当前选定 OCCT Scope 中真实发现的 API
```

如果修改 Scope：

必须在报告注明。

例如：

```text
Previous scope:
TKBRep only

New scope:
TKBRep + TKTopAlgo

Coverage decrease is expected because denominator changed.
```

禁止悄悄改变统计范围。

---

# 37. 不支持 API 也属于项目状态

Unsupported 不代表失败。

正确状态：

```text
Supported
Partially Supported
Manual Wrapper
Unsupported
Ignored by Design
Deprecated
Blocked
```

必须说明原因。

---

# 38. 文件与报告建议结构

```text
/
├── AI_INSTRUCTIONS.md
│
├── docs/
│   ├── STATUS.md
│   ├── ARCHITECTURE.md
│   ├── TYPE_MAPPING.md
│   ├── OWNERSHIP.md
│   ├── SPECIAL_CASES.md
│   ├── KNOWN_ISSUES.md
│   ├── COMPATIBILITY.md
│   ├── DECISIONS.md
│   │
│   └── adr/
│       ├── 0001-*.md
│       └── ...
│
├── reports/
│   ├── binding-report.md
│   ├── binding-report.json
│   ├── test-report.md
│   ├── unsupported-api.md
│   └── upgrade-report.md
│
├── src/
│   ├── Generator/
│   ├── Native/
│   │   ├── Generated/
│   │   └── Manual/
│   └── Managed/
│       ├── Generated/
│       └── Manual/
│
└── tests/
    ├── Generator.Tests/
    ├── Binding.Tests/
    ├── Runtime.Tests/
    ├── Integration.Tests/
    └── TestData/
```

---

# 39. STATUS.md 中必须有 Do Not Change

例如：

```text
## Do Not Change

- Native exception must not cross ABI.
- Handle<T> ownership model O001-O008.
- Generated files must not be manually edited.
- TopoDS_Shape uses dedicated wrapper strategy.
- Standard_CString is copied into managed string.
```

这样后续 AI 不得轻易推翻已有核心规则。

---

# 40. 提交/任务完成报告格式

每次较大的 AI 修改完成后，至少报告：

```text
## Changed

- ...

## Generated

- Classes:
- Methods:

## Coverage Change

Before:
After:

## Tests

Generator:
Native Compile:
Managed Compile:
Runtime:
Integration:

## New Unsupported APIs

- ...

## New TypeMaps

- ...

## Ownership Changes

- None / ...

## Known Issues

- ...

## Documentation Updated

- ...

## Next Recommended Task

- ...
```

---

# 41. 优先修复“可泛化问题”

如果生成失败统计如下：

```text
Unknown TypeMap: 4300
Unsupported Template: 12000
One special class failure: 1
```

应优先分析：

```text
Unknown TypeMap
Unsupported Template
```

因为修复一个泛化规则可能一次支持数千 API。

AI 不应只选择最容易完成的单个类来制造进度。

---

# 42. 不允许无意义追求 100%

以下 API 可以合理 Ignore：

```text
Internal-only
Deprecated
Compiler-specific
Unsafe callback
Impossible ABI
Unused low-level allocator
Unsupported platform feature
```

但必须：

```text
Ignored by Design
```

并给出原因。

Coverage 报告应允许：

```text
Raw Coverage
Supported Scope Coverage
```

二者不能混淆。

---

# 43. 安全优先于覆盖率

当两者冲突时，优先级：

```text
Memory Safety
>
Correct Ownership
>
Correct Semantics
>
Runtime Stability
>
API Coverage
>
Convenience
```

禁止为了 Coverage 自动暴露危险 API。

---

# 44. 用户业务目标优先

如果用户当前核心需求是：

```text
STEP 读取
STEP 写出
多个 STEP 组合
TopoDS 遍历
Face / Edge 获取
轮廓投影
Mesh
Visualization
```

则优先把这些链路做到：

```text
Generated
Compile PASS
Runtime PASS
Integration PASS
Real File PASS
```

而不是先追求冷门模块数量。

---

# 45. AI 遇到不确定设计时

不得随意选择并隐藏不确定性。

应：

1. 检查现有 Decision。
2. 检查 Ownership。
3. 检查 TypeMap。
4. 检查 OCCT 原始 API 语义。
5. 尽量做最小、可回滚设计。
6. 将重要选择写入 Decision。

---

# 46. 核心质量标准

本项目判断“封装成熟度”时，不以代码行数判断。

主要指标：

```text
API Binding Coverage
Runtime Validation Coverage
Ownership/Lifetime Correctness
Integration Test Pass Rate
Real CAD File Validation
OCCT Version Compatibility
Generator Reproducibility
```

---

# 47. 最终原则

AI 必须牢记：

```text
自动生成的 API 数量不是最终目标。

可重新生成、
可升级、
可测试、
生命周期正确、
真实项目可以稳定使用，

才是这个 OCCT .NET Wrapper 项目的目标。
```

---

# 48. 强制执行摘要

每个 AI Agent 必须遵守：

```text
1. 先读状态和架构，再改代码。
2. 不直接修改 Generated 文件。
3. C++ 使用 Clang AST，不用 Regex 当主 Parser。
4. 所有 TypeMap 集中管理。
5. Ownership/Lifetime 是最高优先级。
6. Handle<T> 必须正确处理 OCCT 引用计数。
7. TopoDS_* 必须作为特殊类型认真验证。
8. C++ Exception 不得跨 Native ABI。
9. 每个 Skip API 必须有原因。
10. API Coverage、代码 Coverage、Runtime Coverage 分开统计。
11. 生成后必须实际编译。
12. 核心 API 必须 Runtime Test。
13. 核心业务必须 Integration + Real File Test。
14. 新设计必须记录 Decision。
15. 新特殊类型必须记录 Special Case。
16. 每次任务后更新 STATUS。
17. 没运行的测试必须标记 NOT RUN。
18. 禁止伪造 PASS、Coverage 或完成度。
19. 安全和语义正确优先于覆盖率。
20. 优先解决可泛化问题，而不是堆单个 API 数量。
```
