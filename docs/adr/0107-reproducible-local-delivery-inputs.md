# ADR-0107: reproducible local delivery inputs

Status: Accepted, 2026-10-07.

Native bootstrapping uses a named mutex derived from the canonical workspace path.
Release and Debug share a configure directory, so they share the same lock. Within
the Windows logon session, ensure-native serializes dependency preparation,
configure, build and output validation. build.ps1 uses the same lock around
generation and native compilation, releasing it before managed builds can invoke
ensure-native. Exceptions always release ownership; abandoned mutex recovery runs
the complete configure/build sequence. This does not promise concurrent managed
MSBuild output safety for arbitrary unrelated commands.

pack.ps1 serializes pack invocations with a separate workspace mutex. After optional
native build it freezes source/configuration/document/runtime inputs with hashes and
read-only file handles, refreshes managed assemblies once, then freezes those payloads.
Existing files cannot be written, deleted or renamed during this interval. A watcher
and final path-set/hash comparison reject additions and transient changes, even if
later reverted. Watcher overflow fails the pack. Output directories and volatile
STATUS are excluded. Windows one-sided rename notifications classify the known side;
empty paths are not source files. The source selector is shared by enumeration and
event classification, including untracked source documents.

All fourteen projects pack without rebuilding into a unique staging directory.
Documents, native files and managed payloads are compared to frozen input hashes.
Only a successful complete candidate set reaches the ordinary package directory.
A final package-input manifest binds all source identities and all fourteen package
hashes; provenance includes this manifest, and release checksums include its copy
and all fourteen packages. The manifest is the completion marker, not the mere
presence of a package filename. Promotion is not a filesystem transaction; consumers
must verify the complete manifest, especially after an interrupted copy.

The SDK remains pinned. AO can preserve a precise exclusion when required SDK
headers or exports are absent. Configuration exclusions, parser failures, missing
symbols and intentionally private interfaces are reported separately. No synthetic
dependency headers, new native runtime dependency or SDK upgrade is implicit.
