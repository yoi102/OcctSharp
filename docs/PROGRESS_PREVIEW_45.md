# Progress review: Preview.45 local delivery (2026-10-08)

AP–AT 已交付；AU 的完整本地检查已通过。AU final freeze 为 PASS 后，本轮计为 **6/6**。
最终证据是 OcctSharp/artifacts/batch-au/delivery-result.json，不以文档文字替代冻结结果。
下一轮 AV → AW → AX → AY 已授权连续执行，各批先冻结精确准入。

本轮累计 299 generated / 9 Manual，合计 308 项；AU 新增 41 generated / 0 Manual。
213 主候选中 39 准入 / 174 暂缓，2 新前置、19 复用。226 个旧生成文件字节不变。
116,272 库存声明：26,710 Emitted / 1,227 Manual / 38,988 Blocked / 49,347 Skipped。
非 Skipped 覆盖率 41.7437%，全库存 24.0273%；Pending0。同 32 parser exclusions，
完整语义扫描未完成。声明覆盖率不等于产品完成度。

| 模块 | 已覆盖 Emitted + Manual | Blocked 候选池 | 非 Skipped 覆盖率 |
|---|---:|---:|---:|
| Foundation | 3,535 | 7,360 | 32.45% |
| Geometry | 6,353 | 5,373 | 54.18% |
| Modeling | 3,538 | 11,492 | 23.54% |
| MeshData | 161 | 138 | 53.85% |
| Mesh | 179 | 311 | 36.53% |
| Documents | 1,599 | 2,339 | 40.6% |
| DataExchange | 7,679 | 7,997 | 48.99% |
| Xde | 640 | 728 | 46.78% |
| Visualization | 4,253 | 3,231 | 56.83% |
| IVtk | 0 | 0 | N/A |
| Draw | 0 | 19 | 0.0% |

Release / Debug / 冷源码：每条 Generator263/263、Runtime1025/1025。
实际 Debug-native Runtime1025/1025；62 实际加载 DLL 路径/哈希通过。
229 生成文件冷源码字节一致；ABI/布局/寿命、46 私有头、导出/API 兼容通过。
原生导出 41,323（+8/-0，Debug parity）；API 57,025
（+100/-0）；4269 forwarders。14 包及两个干净消费者通过。

当前桌面环境下，实际 HWND 点击和矩形选择产生可见桌面像素变化；XCAF 保存重开、
精确 occurrence 来源、复制身份及 client resize 失效同时通过。之前的 DXGI 超时、
WGC 无有效客户端变化和独立色块对照保留在 artifacts；未确认其原因，不能声称已修复
驱动问题。没有跳过或放宽可见帧断言，也没有用 ToPixMap 代替桌面证据。

外部 CAD/AS 扩展互操作、Hosted CI、签名、push、公开 NuGet、整下游应用验收：NOT RUN。
详见 [下一轮授权计划](PROGRESS_REVIEW_AND_NEXT_PLAN_2026_10_08.md)。
