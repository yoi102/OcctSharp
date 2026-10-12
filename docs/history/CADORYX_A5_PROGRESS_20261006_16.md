# A5 history before SDK18 cold generation, packages and application integration

## docs/STATUS.md

2026-10-06. Latest accepted A4/B17/C5/D5/E4/F5g; A5 NOT_ACCEPTED, A-F IN_PROGRESS, G deferred. Formal application remains SDK16/freeze11 with full2928, two20-cycle direct target and strictOpen passes.

Isolated SDK18 now passes original Release and actual Debug builds and207 Generator/821 Runtime cases with zero failures/skips and exact case parity. Both self-observation and external observation bind62 actual Debug DLLs;61 dependencies match formal16 Debug, and the bridge differs from Release. All977 source inputs and175 generated outputs remain unchanged. Debug attempt1 lost its process/handle without terminal; cause unknown and evidence retained. The initial external observer used Release names and omitted Debug TBB; the corrected Debug-specific closure contains62 matching modules.

All51 private headers compile independently with /Zs /W4 /WX and no PCH. Release/Debug export names match:41139, +53/-0 versus formal16. Public API snapshots of14 assemblies compare formal16, complete17 and both18 configurations. SDK18 Debug/Release signatures equal17;7 new types and associated signatures are confined to Geometry and its facade, with other modules unchanged from16.

Fresh cold clone starts with all175 outputs and manifest absent, copied from977 frozen18 source inputs. Original build is running in session36599; terminal and byte-identical regeneration remain unverified. Inherited238 metadata files are16 build context, not18 package evidence. Independent NuGet consumer110 is prepared, retaining all78 assertions plus31 bulk/curve ownership cases and1 native alias regression; package/build/runtime remain NOT RUN. Next: finish cold gate, package metadata/local packages/consumer, formal application integration and material/graphics failure scenarios. SDK evidence does not replace application or physical input acceptance.

Evidence: [actual Debug](../../OcctSharp/artifacts/cadoryx-a5/sdk18-debug-verification-1.json), [headers and API compatibility](../../OcctSharp/artifacts/cadoryx-a5/sdk18-interface-verification-1.json), [cold preparation](../../OcctSharp/artifacts/cadoryx-a5/sdk18-cold-regeneration-preparation-1.json), [cold observation](../../OcctSharp/artifacts/cadoryx-a5/sdk18-cold-regeneration-live-1.json), [consumer110 preparation](../../OcctSharp/artifacts/cadoryx-a5/package-consumer-source-18-1.json), [interrupted attempt](../../OcctSharp/artifacts/cadoryx-a5/sdk-bulk-alias-debug-1-interrupted.json).

Previous summary preserved in [history](CADORYX_A5_PROGRESS_20261006_15.md).

## docs/CADORYX_A5_SOURCE_TESSELLATION.md

2026-10-06. Latest accepted A4/B17/C5/D5/E4/F5g; A5 NOT_ACCEPTED, A-F IN_PROGRESS, G deferred. Formal application remains SDK16/freeze11 with full2928, two20-cycle direct target and strictOpen passes.

Isolated SDK18 now passes original Release and actual Debug builds and207 Generator/821 Runtime cases with zero failures/skips and exact case parity. Both self-observation and external observation bind62 actual Debug DLLs;61 dependencies match formal16 Debug, and the bridge differs from Release. All977 source inputs and175 generated outputs remain unchanged. Debug attempt1 lost its process/handle without terminal; cause unknown and evidence retained. The initial external observer used Release names and omitted Debug TBB; the corrected Debug-specific closure contains62 matching modules.

All51 private headers compile independently with /Zs /W4 /WX and no PCH. Release/Debug export names match:41139, +53/-0 versus formal16. Public API snapshots of14 assemblies compare formal16, complete17 and both18 configurations. SDK18 Debug/Release signatures equal17;7 new types and associated signatures are confined to Geometry and its facade, with other modules unchanged from16.

Fresh cold clone starts with all175 outputs and manifest absent, copied from977 frozen18 source inputs. Original build is running in session36599; terminal and byte-identical regeneration remain unverified. Inherited238 metadata files are16 build context, not18 package evidence. Independent NuGet consumer110 is prepared, retaining all78 assertions plus31 bulk/curve ownership cases and1 native alias regression; package/build/runtime remain NOT RUN. Next: finish cold gate, package metadata/local packages/consumer, formal application integration and material/graphics failure scenarios. SDK evidence does not replace application or physical input acceptance.

Evidence: [actual Debug](../../OcctSharp/artifacts/cadoryx-a5/sdk18-debug-verification-1.json), [headers and API compatibility](../../OcctSharp/artifacts/cadoryx-a5/sdk18-interface-verification-1.json), [cold preparation](../../OcctSharp/artifacts/cadoryx-a5/sdk18-cold-regeneration-preparation-1.json), [cold observation](../../OcctSharp/artifacts/cadoryx-a5/sdk18-cold-regeneration-live-1.json), [consumer110 preparation](../../OcctSharp/artifacts/cadoryx-a5/package-consumer-source-18-1.json), [interrupted attempt](../../OcctSharp/artifacts/cadoryx-a5/sdk-bulk-alias-debug-1-interrupted.json).

Previous summary preserved in [history](CADORYX_A5_PROGRESS_20261006_15.md).

## docs/CADORYX_A5_PACKAGE_CONTRACT.md

2026-10-06. Latest accepted A4/B17/C5/D5/E4/F5g; A5 NOT_ACCEPTED, A-F IN_PROGRESS, G deferred. Formal application remains SDK16/freeze11 with full2928, two20-cycle direct target and strictOpen passes.

Isolated SDK18 now passes original Release and actual Debug builds and207 Generator/821 Runtime cases with zero failures/skips and exact case parity. Both self-observation and external observation bind62 actual Debug DLLs;61 dependencies match formal16 Debug, and the bridge differs from Release. All977 source inputs and175 generated outputs remain unchanged. Debug attempt1 lost its process/handle without terminal; cause unknown and evidence retained. The initial external observer used Release names and omitted Debug TBB; the corrected Debug-specific closure contains62 matching modules.

All51 private headers compile independently with /Zs /W4 /WX and no PCH. Release/Debug export names match:41139, +53/-0 versus formal16. Public API snapshots of14 assemblies compare formal16, complete17 and both18 configurations. SDK18 Debug/Release signatures equal17;7 new types and associated signatures are confined to Geometry and its facade, with other modules unchanged from16.

Fresh cold clone starts with all175 outputs and manifest absent, copied from977 frozen18 source inputs. Original build is running in session36599; terminal and byte-identical regeneration remain unverified. Inherited238 metadata files are16 build context, not18 package evidence. Independent NuGet consumer110 is prepared, retaining all78 assertions plus31 bulk/curve ownership cases and1 native alias regression; package/build/runtime remain NOT RUN. Next: finish cold gate, package metadata/local packages/consumer, formal application integration and material/graphics failure scenarios. SDK evidence does not replace application or physical input acceptance.

Evidence: [actual Debug](../../OcctSharp/artifacts/cadoryx-a5/sdk18-debug-verification-1.json), [headers and API compatibility](../../OcctSharp/artifacts/cadoryx-a5/sdk18-interface-verification-1.json), [cold preparation](../../OcctSharp/artifacts/cadoryx-a5/sdk18-cold-regeneration-preparation-1.json), [cold observation](../../OcctSharp/artifacts/cadoryx-a5/sdk18-cold-regeneration-live-1.json), [consumer110 preparation](../../OcctSharp/artifacts/cadoryx-a5/package-consumer-source-18-1.json), [interrupted attempt](../../OcctSharp/artifacts/cadoryx-a5/sdk-bulk-alias-debug-1-interrupted.json).

Previous summary preserved in [history](CADORYX_A5_PROGRESS_20261006_15.md).
