# OcctSharp 开发实施指南

> 当前仓库说明（2026-08-21）：本文件是总体实施背景指南。已确定的架构、目录、
> 路线图和进度以 `docs/DOCUMENTATION_INDEX.md` 索引的专题文档、已接受 ADR 及
> `docs/STATUS.md` 为准。本文第 6 节的旧目录示例已由
> `docs/REPOSITORY_LAYOUT.md` 取代，所有代码相关内容统一放在内部
> `OcctSharp/` 目录。
>
> 项目目标：构建一个可维护、可升级、可测试的 **Open CASCADE Technology（OCCT）→ C#/.NET 自动封装生成器与 SDK**。
>
> 推荐项目名：**OcctSharp**
>
> 推荐核心路线：**Clang AST → Binding Model → C ABI Native Bridge → C# P/Invoke/LibraryImport → Friendly .NET API**

---

## 1. 先明确项目目标

不要把这个项目理解成“写几个 P/Invoke”。

真正要做的是一个小型的 **C++ → .NET Binding Compiler / SDK Generator**。

最终希望达到：

```text
OCCT Headers / Libraries
        ↓
     Clang AST
        ↓
 OcctSharp.Generator
        ↓
  Binding Model
        ↓
┌──────────────────────┐
│ Native Bridge (C ABI)│
│ Managed Raw Binding  │
└──────────────────────┘
        ↓
 Friendly .NET API
        ↓
     OcctSharp.dll
```

以后升级 OCCT 时，理想流程是：

```text
升级 OCCT
↓
重新解析 Headers
↓
重新生成 Binding
↓
查看 API Diff
↓
修正少量特殊规则
↓
编译 + 测试
↓
发布新版本
```

而不是重新手工封几千个类。

---

# 2. 推荐技术路线

## 2.1 长期推荐：C ABI + P/Invoke

如果考虑：

- Windows
- Linux
- macOS
- NativeAOT
- NuGet
- 长期升级

推荐：

```text
OCCT C++
   ↓
OcctSharp.Native
   ↓  stable C ABI
P/Invoke / LibraryImport
   ↓
OcctSharp C#
```

### 优点

- 跨平台更容易。
- 不依赖 C++/CLI。
- Native ABI 完全由自己控制。
- NativeAOT 更友好。
- OCCT 的 C++ ABI 变化不会直接泄漏到 C#。
- NuGet runtime 打包更清晰。

---

## 2.2 Windows-only 可考虑 C++/CLI

如果项目只考虑 Windows/MSVC：

```text
OCCT
 ↓
C++/CLI
 ↓
C#
```

优点是封装 C++ 类比较自然。

缺点：

- 跨平台差。
- NativeAOT 不友好。
- 对 MSVC/Windows 依赖较强。

如果目标是做一个长期通用的 `OcctSharp`，建议优先 **C ABI + P/Invoke**。

---

# 3. 需要用到的技术

建议准备：

```text
C++
Modern C++
C ABI
C#
.NET
P/Invoke
LibraryImport
Clang AST / libclang
CMake
OCCT
NuGet
Git
CI
xUnit / NUnit
BenchmarkDotNet
```

最核心的是：

```text
OCCT
Clang AST
C++ Lifetime / Ownership
Native ABI
P/Invoke
Code Generator
```

---

# 4. C++ 解析必须用 AST

不要使用 Regex 作为主要 C++ Parser。

OCCT Header 中会出现：

```cpp
namespace
template
typedef
using
Handle(T)
inheritance
multiple inheritance
virtual
const
&
*
operator
default parameter
macro
specialization
```

推荐：

- Clang AST
- libclang
- ClangSharp
- CppSharp

Regex 可以辅助处理文本，但不能承担 C++ 语义解析。

---

# 5. Generator 不要直接 AST → C#

建议增加自己的中间层：

```text
Clang AST
    ↓
Binding Model
    ↓
Native Emitter
Managed Emitter
Report Emitter
```

例如：

```csharp
BindingClass
BindingMethod
BindingConstructor
BindingParameter
BindingType
BindingEnum
BindingOwnership
```

这样以后即使改变：

```text
P/Invoke
C++/CLI
Source Generator
```

也不用重写 Parser。

---

# 6. 推荐项目结构

```text
OcctSharp/
│
├── AI_INSTRUCTIONS.md
├── README.md
│
├── docs/
│   ├── STATUS.md
│   ├── ARCHITECTURE.md
│   ├── TYPE_MAPPING.md
│   ├── OWNERSHIP.md
│   ├── SPECIAL_CASES.md
│   ├── KNOWN_ISSUES.md
│   ├── COMPATIBILITY.md
│   └── adr/
│
├── src/
│   ├── OcctSharp.Generator/
│   │   ├── Parsing/
│   │   ├── Ast/
│   │   ├── Model/
│   │   ├── TypeMaps/
│   │   ├── Passes/
│   │   ├── Emitters/
│   │   └── Reports/
│   │
│   ├── OcctSharp.Native/
│   │   ├── Generated/
│   │   ├── Manual/
│   │   └── CMakeLists.txt
│   │
│   ├── OcctSharp/
│   │   ├── Generated/
│   │   ├── Manual/
│   │   ├── Geometry/
│   │   ├── Topology/
│   │   ├── Step/
│   │   ├── Mesh/
│   │   └── Visualization/
│   │
│   └── OcctSharp.Extensions/
│
├── tests/
│   ├── OcctSharp.Generator.Tests/
│   ├── OcctSharp.Runtime.Tests/
│   ├── OcctSharp.Integration.Tests/
│   └── TestData/
│
├── benchmarks/
│   └── OcctSharp.Benchmarks/
│
└── reports/
    ├── binding-report.md
    ├── binding-report.json
    ├── unsupported-api.md
    └── test-report.md
```

---

# 7. Generated 与 Manual 必须彻底分开

```text
Generated/
Manual/
```

## Generated

只能由 Generator 产生。

如果生成错误：

```text
修改 Parser / TypeMap / Pass / Emitter
↓
重新生成
```

禁止直接修改 Generated 文件作为长期修复。

## Manual

只处理：

- 特殊生命周期。
- 特殊模板。
- 高层 Friendly API。
- 性能 Bulk API。
- Viewer 平台集成。
- Callback。
- 其他无法安全自动化的部分。

所有 Manual 特例都要写进：

```text
docs/SPECIAL_CASES.md
```

---

# 8. Native Bridge 不要暴露 C++ ABI

不要这样：

```cpp
__declspec(dllexport)
TopoDS_Shape GetShape();
```

不要直接跨边界暴露：

```text
std::string
std::vector<T>
std::map
C++ class layout
C++ exception
```

推荐统一 C ABI：

```cpp
extern "C"
{
    OCCTSHARP_API void* occtsharp_shape_create();
    OCCTSHARP_API void  occtsharp_shape_destroy(void* handle);
    OCCTSHARP_API int32_t occtsharp_shape_is_null(void* handle);
}
```

C#：

```csharp
[LibraryImport("OcctSharp.Native")]
internal static partial IntPtr occtsharp_shape_create();
```

---

# 9. Native ABI 必须固定这些规则

必须明确：

- Calling convention
- `bool` 大小
- `enum` 大小
- integer 大小
- string encoding
- struct alignment
- ownership
- error handling
- ABI version

推荐 ABI 使用固定类型：

```cpp
int32_t
uint32_t
int64_t
uint64_t
double
```

不要依赖平台相关的：

```cpp
long
bool
```

语义。

---

# 10. 给 Native ABI 做版本号

例如：

```text
OCCTSHARP_NATIVE_ABI_VERSION = 3
```

Managed 加载 Native DLL 时检查版本。

这样可以避免：

```text
OcctSharp.dll 新版本
+
OcctSharp.Native.dll 老版本
```

碰巧能加载，但运行时崩溃。

---

# 11. Ownership / Lifetime 是项目最高风险

每个 Native 对象都必须回答：

```text
谁创建？
谁拥有？
谁释放？
是不是 Borrowed？
是不是 Copy？
是不是 Shared？
是不是 OCCT Handle？
Parent Dispose 后还能不能用？
```

建议 Binding Model：

```csharp
enum OwnershipKind
{
    Value,
    Owned,
    Borrowed,
    Shared,
    Handle,
    Static,
    Unknown
}
```

最重要的规则：

```text
Unknown Ownership
→ 不允许猜
→ Skip
→ Report
→ 人工分析
```

安全优先于 Coverage。

---

# 12. 推荐使用 SafeHandle，但不要滥用

对于明确 Owned 的 native resource，可以考虑：

```csharp
SafeHandle
```

例如：

```csharp
sealed class ShapeHandle : SafeHandle
{
    protected override bool ReleaseHandle()
    {
        Native.ShapeDestroy(handle);
        return true;
    }
}
```

但是：

```text
Borrowed
Shared
Value wrapper
```

不一定适合 SafeHandle。

必须根据 Ownership 决定。

---

# 13. Handle<T> 是 OCCT 封装核心

OCCT 大量存在：

```cpp
Handle(Geom_Curve)
Handle(Geom_Surface)
Handle(AIS_Shape)
```

本质上是 OCCT 的 intrusive reference-counted smart pointer。

不能简单处理为：

```text
T*
→ IntPtr
```

推荐 Native Wrapper 自己持有 Handle：

```cpp
struct GeomCurveHandle
{
    Handle(Geom_Curve) value;
};
```

这样 wrapper 活着时 OCCT 引用计数保持有效。

---

# 14. Handle<T> 必测

至少测试：

```text
Create
Copy
Dispose
Dispose twice
Base → Derived
Derived → Base
Multiple wrappers reference same object
GC finalization
Null Handle
Parent disposed
Exception path
```

---

# 15. TopoDS_Shape 是特殊类型

不要把：

```cpp
TopoDS_Shape
```

当成普通 `Handle<T>`。

它本身更像：

```text
TopoDS_Shape
├─ Handle(TopoDS_TShape)
├─ TopLoc_Location
└─ TopAbs_Orientation
```

所以它具有 Value Wrapper 特征。

以下都应做专门 TypeMap / Ownership 规则：

```text
TopoDS_Shape
TopoDS_Vertex
TopoDS_Edge
TopoDS_Wire
TopoDS_Face
TopoDS_Shell
TopoDS_Solid
TopoDS_CompSolid
TopoDS_Compound
```

必须测试：

- Copy
- IsNull
- Orientation
- Location
- TShape sharing
- subtype conversion
- lifetime

---

# 16. gp_* 类型适合优化成值类型

例如：

```text
gp_Pnt
gp_Pnt2d
gp_Vec
gp_Dir
gp_XYZ
gp_Ax1
gp_Ax2
gp_Trsf
```

这些是高频、小型几何对象。

不要全部做成：

```text
new native object
P/Invoke X
P/Invoke Y
P/Invoke Z
Dispose
```

否则大量几何操作会产生非常高的 Interop 开销。

可以考虑：

```csharp
public readonly struct Point3d
{
    public double X { get; }
    public double Y { get; }
    public double Z { get; }
}
```

具体布局必须确认后再决定是否直接 blittable。

---

# 17. TypeMap 必须集中管理

建立：

```text
docs/TYPE_MAPPING.md
```

示例：

```text
Standard_Integer
→ int

Standard_Real
→ double

Standard_Boolean
→ bool / fixed ABI int

Standard_CString
→ string

gp_Pnt
→ Point3d

TopoDS_Shape
→ Shape

TopoDS_Face
→ Face

Handle<T>
→ Managed reference wrapper
```

不要把 TypeMap 写成散落在各个 Emitter 里的巨大 `if/else`。

推荐：

```csharp
interface ITypeMap
{
    bool CanMap(CppType type);
    ManagedType Map(CppType type);
}
```

例如：

```text
PrimitiveTypeMap
StringTypeMap
HandleTypeMap
TopoDSTypeMap
GpTypeMap
CollectionTypeMap
EnumTypeMap
```

---

# 18. Generator 建议采用 Pass 系统

例如：

```text
IgnorePass
RenamePass
AccessibilityPass
OwnershipPass
TypeMapPass
InheritancePass
OverloadResolutionPass
UnsupportedPass
```

这样后续增加规则不会把 Generator 写成一大坨特殊判断。

---

# 19. String 是高风险区域

必须明确处理：

```text
Standard_CString
Standard_ExtString
TCollection_AsciiString
TCollection_ExtendedString
```

每种字符串都要明确：

```text
Encoding
Nullability
Ownership
Lifetime
```

Native → Managed 通常建议复制。

不要让 C# 长期持有生命周期不明确的：

```text
char*
wchar_t*
```

---

# 20. 字符串测试

至少：

```text
English
中文
日本語
空字符串
Null
长字符串
Unicode 文件路径
特殊字符路径
```

STEP 文件路径尤其要测。

---

# 21. Collection 不能机械转换

OCCT 有：

```text
NCollection_List<T>
NCollection_Vector<T>
NCollection_Sequence<T>
NCollection_Array1<T>
NCollection_Array2<T>
NCollection_Map<T>
NCollection_DataMap<K,V>
```

不能默认：

```text
NCollection_* → List<T>
```

必须考虑：

- 索引是否从 0 开始
- Copy semantics
- Mutation
- Iterator lifetime
- Ownership
- Parent lifetime

尤其 `Array1` 要注意：

```text
Lower()
Upper()
```

可能不是 0-based。

---

# 22. 方法重载要做冲突检测

C++：

```cpp
Foo(int)
Foo(double)
Foo(const T&)
Foo(const Handle(T)&)
```

映射成 C# 后可能发生签名冲突。

Generator 必须检测：

```text
Native overload
↓
Mapped managed signature collision
```

不要生成后让 C# Compiler 才第一次发现。

---

# 23. 默认参数

简单默认参数可以直接生成。

复杂默认参数推荐生成 overload。

例如 C++：

```cpp
Build(const gp_Ax2& axis = gp::XOY());
```

Managed 可以变成：

```csharp
Build();
Build(Axis2 axis);
```

比硬翻复杂 C++ default expression 更稳。

---

# 24. Multiple Inheritance 必须特殊处理

C++：

```text
class X : A, B
```

C# 无法 class 多继承。

可选策略：

```text
Primary base class
Interface projection
Composition
Manual wrapper
Skip
```

不要机械翻译。

---

# 25. RTTI / DownCast

OCCT 中：

```text
Standard_Transient
DynamicType
Handle<T>::DownCast
```

非常重要。

例如：

```text
Handle(Geom_Geometry)
```

真实对象可能是：

```text
Geom_Line
Geom_Circle
Geom_BSplineCurve
```

如果要做高质量 OO Wrapper，需要设计：

```text
Native dynamic type
→ Managed concrete wrapper
```

第一版可以有限支持，但设计上不能完全忽略。

---

# 26. C++ Exception 绝对不能穿越 C ABI

Native Bridge 必须：

```cpp
try
{
    ...
}
catch (const Standard_Failure& ex)
{
    ...
}
catch (const std::exception& ex)
{
    ...
}
catch (...)
{
    ...
}
```

然后转换成：

```text
Native Error Code
+
Error Message
↓
OcctException
```

禁止：

```cpp
catch (...)
{
    return nullptr;
}
```

然后吞掉真实错误。

---

# 27. Callback 第一版不要强求

涉及：

```text
function pointer
delegate
virtual callback
native → managed callback
```

会引入：

- Delegate lifetime
- GC
- Thread
- Exception
- NativeAOT
- reentrancy

第一阶段建议 `Skip + Report`。

---

# 28. Visualization 放后面

OCCT Visualization 涉及：

```text
AIS
V3d
Graphic3d
OpenGl
Aspect
SelectMgr
Prs3d
```

复杂度明显高于：

```text
gp
TopoDS
BRep
STEP
```

不要第一阶段就追求完整 Viewer。

---

# 29. 推荐模块实施顺序

## Phase 0：基础设施

```text
Repository
CMake
.NET solution
固定 OCCT 版本
Native Bridge Demo
Clang parser
CI
```

## Phase 1：基础类型

```text
Standard
gp
TopAbs
TopLoc
TopoDS
TopExp
```

## Phase 2：Geometry / BRep

```text
Geom
Geom2d
BRep
BRepTools
BRepAdaptor
GeomAdaptor
GCPnts
```

## Phase 3：STEP

```text
STEPControl
XSControl
IFSelect
Interface
```

需要 Assembly/Color/Name 时再加入：

```text
STEPCAFControl
XCAF
XDE
TDocStd
TDF
```

## Phase 4：Modeling

```text
BRepAlgoAPI
BRepBuilderAPI
BRepPrimAPI
BRepOffsetAPI
BRepFilletAPI
```

## Phase 5：Mesh

```text
BRepMesh
Poly
```

## Phase 6：XDE

```text
XCAF
STEPCAF
OCAF
```

## Phase 7：Visualization

```text
AIS
V3d
Graphic3d
OpenGl
SelectMgr
```

---

# 30. 第一版不要追求完整 OCCT

不要设：

```text
目标：先生成 OCCT 100% API
```

建议先做真正可用的业务闭环：

```text
STEP
↓
TopoDS_Shape
↓
Face / Edge
↓
Geometry
↓
Mesh
↓
STEP Write
```

一个完整可运行的链路，比生成两万个未经 Runtime Validation 的方法价值更高。

---

# 31. MVP 建议

第一版只需要打通：

```text
gp_Pnt
TopoDS_Shape
TopoDS_Face
TopoDS_Edge
BRepPrimAPI_MakeBox
TopExp_Explorer
STEPControl_Reader
STEPControl_Writer
```

目标 C#：

```csharp
using var box = ShapeFactory.MakeBox(10, 20, 30);

StepWriter.Write(box, "box.step");

using var shape = StepReader.Read("box.step");

foreach (var face in shape.Faces)
{
    Console.WriteLine(face);
}
```

如果这个闭环的：

```text
Generator
Lifetime
ABI
P/Invoke
Dispose
STEP
Topology traversal
Tests
```

全部稳定，核心架构就成立了。

---

# 32. 生成范围初期用白名单

建议：

```text
Parse all headers for discovery
```

但：

```text
Generate only selected packages/modules
```

例如：

```text
gp
TopoDS
TopExp
BRepPrimAPI
STEPControl
```

初期不要：

```text
Generate Everything
→ Ignore 80%
```

会非常难维护。

---

# 33. Generator 必须产生报告

每次生成至少输出：

```text
reports/binding-report.md
reports/binding-report.json
reports/unsupported-api.md
```

包括：

```text
Classes discovered
Classes generated
Methods discovered
Methods generated
Constructors
Enums
Skip count
Skip reasons
Module coverage
```

---

# 34. 每个 Skip 都必须有原因

例如：

```text
UnknownTypeMap
UnsupportedTemplate
MultipleInheritance
FunctionPointer
Variadic
Private
Protected
Deprecated
OwnershipUnknown
ManualIgnore
ParserFailure
GeneratorFailure
```

禁止静默：

```csharp
if (unsupported)
    continue;
```

---

# 35. Coverage 要分开统计

不能只写：

```text
Coverage = 90%
```

必须区分：

```text
Generator Line Coverage
Generator Branch Coverage

Binding Class Coverage
Binding Method Coverage

Compile Validation Coverage
Runtime Validation Coverage
Integration Validation Coverage
Real File Validation Coverage
```

因为：

```text
能生成
≠
能编译
≠
能调用
≠
生命周期正确
≠
真实 CAD 可用
```

---

# 36. Runtime Test 是关键

这个项目最危险的错误往往不是编译错误，而是：

```text
Double Free
Use After Free
Wrong RefCount
Wrong ABI
Wrong bool size
Wrong string lifetime
Wrong borrowed ownership
```

所以 Runtime Test 不能省。

---

# 37. 测试分层

至少建立：

```text
Generator Unit Tests
Generated Native Compile Test
Generated Managed Compile Test
Runtime Binding Tests
Lifetime Tests
Integration Tests
Real File Tests
```

---

# 38. Lifetime Test 必须非常重视

至少：

```text
Dispose
Dispose twice
GC
Shared Handle
Borrowed return
Copy Shape
Parent Dispose
Child Dispose
Exception path
Null object
```

建议还有压力测试：

```text
循环创建 / 销毁 10,000～100,000 次
```

观察：

```text
Crash
Leak
Use-after-free
```

---

# 39. Native 内存诊断

可用时加入：

```text
AddressSanitizer
UndefinedBehaviorSanitizer
```

Windows 也可使用：

```text
Visual Studio Native Diagnostics
Application Verifier
PageHeap
```

帮助检查：

```text
Double Free
Heap corruption
Use After Free
```

---

# 40. Real CAD Test Data

建议：

```text
tests/TestData/
├── simple_box.step
├── bspline.step
├── assembly.step
├── color_xde.step
├── mixed_geometry.step
└── large_model.step
```

分：

```text
Small Unit Assets
Medium Integration Assets
Large Benchmark Assets
```

---

# 41. STEP 与 STEPCAF 要区分

普通：

```text
STEPControl_Reader
```

主要面向 Shape。

如果要：

```text
Assembly
Name
Color
Layer
Metadata
```

通常要引入：

```text
STEPCAFControl_Reader
XDE
XCAF
```

所以 API 设计时要提前考虑两种层次。

---

# 42. 性能最大的坑：Chatty Interop

例如 Mesh 有 1,000,000 个点。

如果每个点：

```csharp
vertex.X
vertex.Y
vertex.Z
```

都是一次 P/Invoke，

就是数百万次 Native Boundary Call。

应设计 Bulk API：

```csharp
mesh.CopyVertices(Span<Point3d> destination);
mesh.CopyTriangles(Span<Triangle> destination);
```

或者一次性复制数组。

---

# 43. 高频数据要批量传输

适合 Bulk API：

```text
Mesh vertices
Triangles
Normals
Polyline points
Curve sampling points
Face triangulation
```

第一阶段优先安全 Copy。

不要过早做 Zero-copy。

---

# 44. 不要过早追求 Zero-copy

Zero-copy 会引入：

```text
Pinned memory
Unsafe
Span lifetime
Native lifetime
GC interaction
Thread safety
```

正确性稳定以后再优化。

---

# 45. Benchmark

建议使用：

```text
BenchmarkDotNet
```

测试：

```text
STEP Read
STEP Write
Face enumeration
Edge enumeration
Mesh extraction
Boolean Fuse
Transform
```

最好同时有：

```text
Native OCCT baseline
Managed wrapper result
```

这样知道 Wrapper overhead。

---

# 46. Native Build 推荐 CMake

原因：

```text
Windows
Linux
macOS
CI
```

统一。

OCCT 依赖可以考虑：

```text
vcpkg
预编译 OCCT
源码构建
```

开发期使用 vcpkg 很方便，但发布 NuGet 时不能假设用户机器已经安装 OCCT。

---

# 47. NuGet 打包必须包含 Native Runtime

用户理想体验：

```bash
dotnet add package OcctSharp
```

然后直接运行。

不能要求用户自己去找：

```text
TKernel.dll
TKMath.dll
TKBRep.dll
TKSTEP.dll
...
```

---

# 48. NuGet Runtime Layout

可以采用：

```text
runtimes/
├── win-x64/
│   └── native/
├── linux-x64/
│   └── native/
└── osx-arm64/
    └── native/
```

Managed：

```text
lib/net8.0/OcctSharp.dll
```

也可以拆：

```text
OcctSharp
OcctSharp.Native.Win-x64
OcctSharp.Native.Linux-x64
OcctSharp.Native.Osx-arm64
```

具体根据发布策略决定。

---

# 49. 检查 Native Dependency

CI 应检查：

Windows：

```text
dumpbin /dependents
```

Linux：

```text
ldd
```

macOS：

```text
otool -L
```

避免只带：

```text
OcctSharp.Native
```

却漏掉 OCCT runtime DLL/SO。

---

# 50. 版本信息必须记录

至少：

```text
OcctSharp Version
Generator Version
Native ABI Version
OCCT Version
Compiler
Architecture
.NET Version
```

Managed 端最好可以查询这些信息。

---

# 51. OCCT 升级流程

必须标准化：

```text
Update OCCT
↓
Parse all headers
↓
AST/API Diff
↓
Regenerate
↓
Binding Diff
↓
Native Compile
↓
Managed Compile
↓
Unit Tests
↓
Runtime Tests
↓
Integration Tests
↓
Real File Tests
↓
Upgrade Report
```

---

# 52. Upgrade Report

自动记录：

```text
Added classes
Removed classes
Changed methods
Changed constructors
Changed inheritance
Changed enums
New unknown TypeMaps
New unsupported templates
Compile regressions
Runtime regressions
```

例如：

```text
OCCT 7.9.x → 8.x

Added Classes: 120
Removed Classes: 8
Changed Methods: 370
New Unknown TypeMap: 42
Compile Failures: 6
```

---

# 53. Generator 必须可重复

同样的：

```text
OCCT Version
Generator Version
Configuration
```

必须产生相同代码。

CI 可以：

```text
Run Generator
git diff --exit-code
```

确认已提交 Generated 内容没有过期。

---

# 54. Generated 输出必须稳定排序

固定：

```text
Class order
Method order
Include order
File order
```

否则每次生成都会产生大量无意义 Git Diff。

---

# 55. API 命名建议

最好分两层。

Raw Binding 保留 OCCT 对应关系：

```text
TopoDSShape
BRepAlgoAPIFuse
STEPControlReader
```

Friendly API：

```text
Shape
BooleanOperations
StepReader
StepWriter
```

这样：

```text
Raw 层便于对照 OCCT 文档
Friendly 层便于 C# 使用
```

---

# 56. 不要只做 Raw Binding

如果 C# 用户为了读一个 STEP 还必须熟悉：

```text
XSControl_WorkSession
Interface_Static
Transfer_Binder
```

说明 Wrapper 易用性不足。

高层 API 应提供：

```csharp
Shape shape = StepReader.Read(path);
```

---

# 57. 也不要只做 High-Level

如果所有 OCCT 能力都必须人工设计 Friendly API，

以后扩 API 会很慢。

最合理：

```text
Generated Raw Binding
+
Manual Friendly API
```

---

# 58. Thread Safety 不要猜

不要默认：

```text
OCCT objects are thread-safe
```

尤其：

```text
Interface_Static
Document
Viewer Context
Global state
Allocator
```

可能有线程安全限制。

没有证据就不要在 Wrapper 文档宣称 Thread Safe。

---

# 59. 内存分配必须同侧释放

禁止：

```text
C++ new
→ C# free
```

或者：

```text
malloc
→ delete
```

原则：

```text
谁分配，谁释放。
```

Native 分配的资源必须提供对应 Native Release API。

---

# 60. Debug Handle Registry 很有价值

开发期可以记录：

```text
Handle Type
Pointer
Create
Destroy
Owning/Borrowed
```

程序结束后输出：

```text
Remaining Native Handles
```

可以快速发现 Wrapper Leak。

Debug 模式做即可。

---

# 61. License 要提前确认

如果准备开源/发布 NuGet，必须确认：

```text
OCCT License
Bundled Native Binary Redistribution
Third-party License
Generated Code License
```

建议准备：

```text
LICENSE
NOTICE
THIRD_PARTY_NOTICES
```

不要等发布前才处理。

---

# 62. AI 开发时最容易犯的错误

重点防止：

```text
为了能编译全部映射 IntPtr
猜 Ownership
乱 delete borrowed pointer
忽略 Handle<T> refcount
把 TopoDS 当普通 pointer
直接修改 Generated
遇到一个类就增加一次性 hack
重复建立不同 TypeMap
为了 Coverage 隐藏 Skip
没跑测试却报告 PASS
编译成功就声称 Runtime Stable
```

---

# 63. AI 遇到一个未知类型时应该做什么

错误方式：

```text
Unknown FooType
→ 暂时映射成 IntPtr
```

正确流程：

```text
FooType 是 class / value / typedef / Handle / collection？
↓
谁拥有？
↓
如何传参？
↓
如何返回？
↓
有没有父类？
↓
是否需要专用 TypeMap？
↓
增加测试
↓
更新 TYPE_MAPPING.md
```

---

# 64. 优先解决可泛化问题

如果 Report：

```text
Unknown TypeMap        4,300
Unsupported Template  12,000
Special Class Error        1
```

不要总挑一个最容易的类手工修。

优先分析：

```text
Unknown TypeMap
Unsupported Template
```

一个通用规则可能一次解锁几千 API。

---

# 65. 但是优先级最高的是安全

建议优先级：

```text
Memory Safety
>
Ownership / Lifetime
>
Correct ABI
>
Correct Semantics
>
Runtime Stability
>
Integration
>
API Coverage
>
Performance
>
Convenience
```

---

# 66. 推荐里程碑

## M0 — Hello OCCT

```text
OCCT dependency works
Native bridge builds
C# calls one OCCT-backed native function
```

## M1 — Generator Skeleton

```text
Clang AST
Binding Model
Basic Emitter
Primitive TypeMap
Enum
Simple method
```

## M2 — Lifetime Foundation

```text
Handle<T>
TopoDS
gp
SafeHandle / ownership framework
Lifetime tests
```

## M3 — STEP Closed Loop

```text
STEP Read
STEP Write
Shape traversal
Real STEP test
```

## M4 — Modeling

```text
Geometry
BRep
Boolean
Transform
```

## M5 — Mesh

```text
Triangulation
Bulk APIs
Benchmark
```

## M6 — XDE

```text
Assembly
Color
Name
Layer
```

## M7 — Visualization

```text
AIS
Viewer
Selection
```

## M8 — Distribution

```text
Windows/Linux/macOS
NuGet
CI matrix
Compatibility report
```

---

# 67. 第一阶段成功标准

不要用：

```text
Generated 5000 classes
```

作为成功标准。

应该是：

```text
Generator reproducible                  PASS
Native Build                            PASS
Managed Build                           PASS
Lifetime Tests                          PASS
STEP Read                               PASS
STEP Write                              PASS
Shape Traversal                         PASS
Real STEP File                          PASS
Binding Report                          PASS
Unsupported API Report                  PASS
No known critical lifetime bug
```

---

# 68. 推荐第一条完整 Demo

第一阶段最终应该至少可以：

```csharp
using OcctSharp;
using OcctSharp.Modeling;
using OcctSharp.Step;

using Shape box =
    ShapeFactory.MakeBox(10, 20, 30);

StepWriter.Write(
    box,
    "box.step");

using Shape loaded =
    StepReader.Read("box.step");

Console.WriteLine(
    $"Faces: {loaded.Faces.Count}");

Console.WriteLine(
    $"Edges: {loaded.Edges.Count}");
```

如果这段背后的：

```text
Generator
Native ABI
TypeMap
Ownership
Dispose
TopoDS
STEP
Topology enumeration
Integration Test
```

全部稳定，说明架构已经基本成立。

---

# 69. 正式开工顺序

建议严格按下面开始：

```text
1. 创建 OcctSharp repository
2. 建立 .NET solution
3. 建立 Native CMake project
4. 固定 OCCT 版本
5. C++ 手工写一个最小 C ABI
6. C# 成功 P/Invoke 调用
7. 接入 Clang AST
8. 建立 Binding Model
9. 支持 Primitive
10. 支持 Enum
11. 支持简单 Constructor / Method
12. 自动生成 Native Bridge
13. 自动生成 Managed Binding
14. 建立 TypeMap 系统
15. 建立 Ownership 模型
16. 支持 Handle<T>
17. 支持 gp_Pnt
18. 支持 TopoDS_Shape
19. 支持 BRepPrimAPI_MakeBox
20. 支持 TopExp_Explorer
21. 支持 STEPControl_Reader
22. 支持 STEPControl_Writer
23. 做 Real STEP Integration Test
24. 做 Lifetime Stress Test
25. 生成 Binding Coverage Report
26. 再开始扩大模块
```

---

# 70. 每次开发任务完成检查

```text
[ ] Generator 可执行
[ ] Generated Native 编译
[ ] Generated Managed 编译
[ ] Unit Tests
[ ] Runtime Tests
[ ] Lifetime Tests
[ ] Integration Tests
[ ] 更新 Binding Coverage
[ ] 更新 Skip Reasons
[ ] 新 TypeMap 已记录
[ ] Ownership 变化已记录
[ ] Special Case 已记录
[ ] STATUS.md 已更新
[ ] Generated 没有被直接手改
```

未执行必须写：

```text
NOT RUN
```

不能写成 PASS。

---

# 71. 最重要的 25 条注意事项

```text
1. 不要 Regex 解析完整 C++。
2. 使用 Clang AST。
3. AST 与生成代码之间建立 Binding Model。
4. Generated / Manual 严格分离。
5. 不直接向 C# 暴露 C++ ABI。
6. 推荐稳定 C ABI + P/Invoke/LibraryImport。
7. 给 Native ABI 做版本检查。
8. Ownership/Lifetime 优先级最高。
9. Unknown Ownership 不允许猜。
10. Handle<T> 必须保持 OCCT 引用计数语义。
11. TopoDS_* 需要特殊处理。
12. gp_* 高频小对象考虑 Value/Bulk 优化。
13. 字符串 Encoding/Lifetime 必须明确。
14. NCollection 不能机械变成 List<T>。
15. Multiple Inheritance 必须特殊处理。
16. C++ Exception 不能穿越 ABI。
17. Callback 第一阶段可以 Skip。
18. Viewer 放后期。
19. 编译通过不代表 Runtime 正确。
20. 必须大量做 Lifetime Test。
21. 避免 Chatty P/Invoke。
22. Mesh 等大量数据必须设计 Bulk API。
23. API Coverage 与 Test Coverage 分开。
24. 每个 Skip 必须有原因。
25. OCCT 升级必须能够自动 Diff + Regenerate + Validate。
```

---

# 72. 推荐最终技术栈

```text
Native:
  C++20 或项目统一现代 C++ 标准
  CMake
  OCCT

Parser:
  Clang AST / libclang
  ClangSharp 或 CppSharp 可作为候选

Managed:
  C#
  .NET 8+（根据目标环境决定）

Interop:
  C ABI
  LibraryImport / PInvoke

Tests:
  xUnit
  Native Runtime Tests
  Integration Tests
  Real CAD Files

Diagnostics:
  AddressSanitizer
  Native Handle Registry

Benchmark:
  BenchmarkDotNet

Dependency:
  vcpkg 或受控 OCCT build

CI:
  GitHub Actions

Distribution:
  NuGet
```

---

# 结论

这个项目真正难的并不是：

```text
“C# 怎么调用一个 OCCT C++ 函数？”
```

真正难的是长期保证：

```text
C++ 类型语义正确
Ownership 正确
Lifetime 正确
Handle<T> 正确
TopoDS 正确
ABI 稳定
生成结果可重复
运行测试真实有效
性能不过度损失
OCCT 升级后可以重新生成
```

因此从第一天就应该把下面五件事当作核心：

```text
Generator
+
Type/Ownership Rules
+
Stable Native ABI
+
Runtime/Lifetime Validation
+
Coverage & Upgrade Reports
```

不要把 OcctSharp 当成普通的 P/Invoke 项目。

应该把它设计成一个：

**OCCT C++ → .NET Binding Generator + Managed SDK。**
