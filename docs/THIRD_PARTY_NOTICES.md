# Third-Party Notices and Redistribution Record

OcctSharp project code is licensed under MIT. The committed Windows x64 runtime below
`OcctSharp/runtime/win-x64/` contains third-party components that remain governed by
their respective licenses. The package carries the same notice and license tree.

| Component | Version / identity | DLLs | License evidence |
|---|---|---|---|
| Open CASCADE Technology | 8.0.1 VC14 x64 | `TKernel.dll`, `TK*.dll` | LGPL-2.1-only plus OCCT exception in `licenses/occt/` |
| oneTBB | 2021.13.0 | `tbb12.dll` | Apache-2.0 text in `licenses/onetbb/` |
| FreeImage | 3.18.0 | `FreeImage.dll` | FreeImage Public License and GPL alternatives in `licenses/freeimage/` |
| FreeType | 2.13.3 | `freetype.dll` | FreeType License/GPL notice in `licenses/freetype/` |
| OpenVR | 1.14.15 | `openvr_api.dll` | BSD-3-Clause text from upstream tag `v1.14.15` in `licenses/openvr/` |
| FFmpeg | 3.3.4 shared build | `avcodec-57.dll`, `avformat-57.dll`, `avutil-55.dll`, `swscale-4.dll` | Binary configuration reports LGPL 2.1 or later and no GPL enable flag; upstream `n3.3.4` LGPL text in `licenses/ffmpeg/` |
| jemalloc | supplied bundle build; usable version metadata unavailable | `jemalloc.dll` | Bundled BSD-style copyright/license text in `licenses/jemalloc/` |

`OcctSharp.Native.dll` is built from this MIT-licensed repository. Every DLL and notice
file is pinned by path, byte size, and SHA256 in
`OcctSharp/runtime/win-x64/runtime-manifest.json`; verification fails on missing, extra,
or changed files. The jemalloc version is deliberately recorded as unavailable because
the supplied distribution contains placeholder build metadata rather than a trustworthy
version identifier.

This record documents the redistributed material and included upstream terms. It does
not relicense third-party components under MIT.
