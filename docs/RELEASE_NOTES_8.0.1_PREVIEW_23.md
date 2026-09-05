# 8.0.1-preview.23: Broad generated numeric and reference migration

Batch X expands generated projections under [ADR-0092](adr/0092-generated-numeric-and-copied-reference-expansion.md).
It adds 3,329 generated declaration IDs (16,353 to 19,682), with no generated removals
and no new manual binding IDs. The public managed comparison adds 3,287 signatures
and removes none. [STATUS](STATUS.md) records current validation and completion evidence.

TM008 supports canonical float and signed/unsigned fixed-width integer types, Windows
long, size_t and aliases, plus const-reference numeric inputs. Known scalar, enum and
gp_Pnt reference results are copied. Selected const intrusive-handle results receive
their own registered owner, retaining native sharing and surviving source disposal.

Examples include byte arrays in Documents, unsigned mesh-purpose masks and retained
triangulation parameters, image dimensions and half-float conversion, copied AIS label
positions and 64-bit camera state counters. The breadth includes runtime type handles:
2,587 new declaration IDs are attributed to the Standard source package, predominantly
macro-origin DynamicType methods. Declaration count is not a count of independent CAD
features, and full OCCT migration remains incomplete.

The old generated overload ordinals, C signatures and function bodies are preserved.
New static entry points contain C++ exceptions and use status plus initialized output;
old direct-return entry points remain compatible. Plain char, long double, arbitrary
pointers, mutable/out references and general borrowed object returns are not added.
SC-061 records the absent Windows Cocoa export and the BSplCLib array-start reference;
neither is counted as migrated.

Package 8.0.1-preview.23 / ABI 1.67 / bridge 0.75.0 retain OCCT 8.0.1, configuration
schema 1.13, binding-model schema 1.3 and assembly/file 0.1.0.0. Twelve managed modules,
the facade and the one shared native runtime package keep the same dependency graph.
This is a local preview; hosted CI, signing, NuGet publication and GitHub push are
outside this task. MIT applies to OcctSharp code; bundled dependencies retain their
licenses listed in [third-party notices](THIRD_PARTY_NOTICES.md).
