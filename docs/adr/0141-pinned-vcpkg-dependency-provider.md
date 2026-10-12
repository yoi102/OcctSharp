# ADR-0141: pinned independent vcpkg dependency provider

Status: accepted within the user's authorized reproducible dependency recovery.
Date: 2026-10-12.

The existing local SDK requires a machine-specific precompiled OCCT/third-party
layout. A fresh environment needs a versioned source recovery route without
changing the qualified public runtime payload or weakening its exact closure.

Add a manifest provider with a fixed vcpkg registry/tool, OCCT source archive,
features and triplets. Retain the original SDK header bytes through a bounded
OCCT overlay so the generator's exact admission contracts still apply. Enable
D3D required by the bridge and explicitly disable OpenVR. The initial profile
also disabled FFmpeg; complete Runtime validation disproved that assumption:
OCCT's native Media time conversions returned0 without the feature. The v2
profile requests fixed FFmpeg9.0.2#1 avcodec/avformat/swscale; the pinned port
also resolves avdevice/avfilter/swresample. This preserves the
existing native call and signed64-bit assertions. The failed2253/2255 run stays
immutable. The revised profile must pass its own empty source restore, builds,
full runtime, provenance and license gates.

The core port omits183 optional toolkit headers referenced by installed legacy
aliases and generated shared-handle declarations. Recover those original bytes
from the fixed source archive using explicit archive paths and header hashes.
The complete7478-header SDK view feeds both generation and native compilation;
its manifest and every file hash are rechecked before building. The linked
library graph remains the core vcpkg targets. Header recovery does not qualify
optional Draw/IVtk runtime behavior or change declaration accounting.

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
