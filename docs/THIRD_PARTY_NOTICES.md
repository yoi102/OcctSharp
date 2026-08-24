# Third-Party Notices and Review Status

The local alpha.41 package is not approved for public redistribution. This inventory is
a release gate, not a declaration that every license review is complete.

## Open CASCADE Technology

The package contains OCCT 8.0.1 toolkit DLLs. OCCT is distributed under GNU LGPL 2.1
with the Open CASCADE exception. The package carries the upstream
`LICENSE_LGPL_21.txt` and `OCCT_LGPL_EXCEPTION.txt` files under `licenses/occt/`.

## Other native files observed in the pinned distribution

The current 47-DLL closure also contains `tbb12.dll`, `jemalloc.dll`, `freetype.dll`,
`FreeImage.dll`, `openvr_api.dll`, and FFmpeg-family `avcodec-57.dll`,
`avformat-57.dll`, `avutil-55.dll`, and `swscale-4.dll`. Their exact upstream versions,
build options, source offers where applicable, and redistribution notices are not present
in the supplied artifact and remain `UNRESOLVED` for public release.

`OcctSharp.Native.dll` is built from this repository. The repository itself does not yet
have a user-selected project license, so public package publication remains blocked by
PD-012 even though local build and package testing pass.

The generated CycloneDX SBOM records every native filename and SHA256. A complete legal
review must replace each `unknown` third-party version/license entry before publication.
