# ADR-0141: pinned independent vcpkg dependency provider

Status: accepted within the user's authorized reproducible dependency recovery.
Date: 2026-10-12.

The existing local SDK requires a machine-specific precompiled OCCT/third-party
layout. A fresh environment needs a versioned source recovery route without
changing the qualified public runtime payload or weakening its exact closure.

Add a manifest provider with a fixed vcpkg registry/tool, OCCT source archive,
features and triplets. Retain the original SDK header bytes through a bounded
OCCT overlay so the generator's exact admission contracts still apply. Enable
D3D required by the bridge and explicitly disable unused FFmpeg/OpenVR.

Keep the registry checkout outside the repository and the installed/build
artifacts under the code workspace or an explicitly selected machine cache.
Generation runs in an isolated output directory and must match current public
generated files. Native compilation and recursive PE import resolution use an
independent output root and exact configuration-specific DLL manifests.

Do not conflate provider identities or assume the historical 62 DLLs for a new
dependency graph. Preserve all existing precompiled provider gates. Restoring,
compiling, runtime acceptance and package publication remain separate statuses;
vcpkg becomes a publication provider only after independent full qualification.

See [recovery instructions](../DEPENDENCY_RECOVERY.md).
