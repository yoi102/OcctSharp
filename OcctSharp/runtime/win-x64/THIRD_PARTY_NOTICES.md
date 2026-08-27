# OcctSharp Windows x64 Runtime Notices

This directory contains the application-local Windows x64 runtime distributed with
OcctSharp. The project code and `OcctSharp.Native.dll` are licensed under the MIT
License at the repository root. The remaining DLLs are redistributed under their own
licenses; the corresponding upstream texts are included below `licenses/`.

| Component | Version / identity | Runtime files | License | Included text |
|---|---|---|---|---|
| Open CASCADE Technology | 8.0.1 VC14 x64 binary distribution | `TKernel.dll`, `TK*.dll` | LGPL-2.1-only with OCCT exception | `licenses/occt/` |
| oneTBB | 2021.13.0 | `tbb12.dll` | Apache-2.0 | `licenses/onetbb/LICENSE.txt` |
| FreeImage | 3.18.0 | `FreeImage.dll` | FreeImage Public License / GPL dual license | `licenses/freeimage/` |
| FreeType | 2.13.3 | `freetype.dll` | FreeType License / GPL dual license | `licenses/freetype/LICENSE.TXT` |
| OpenVR | 1.14.15 | `openvr_api.dll` | BSD-3-Clause | `licenses/openvr/LICENSE` |
| FFmpeg | 3.3.4 shared build; embedded configuration reports LGPL 2.1 or later and no GPL enable flag | `avcodec-57.dll`, `avformat-57.dll`, `avutil-55.dll`, `swscale-4.dll` | LGPL-2.1-or-later | `licenses/ffmpeg/COPYING.LGPLv2.1` |
| jemalloc | Bundled build; upstream version metadata is unavailable in the supplied binary bundle | `jemalloc.dll` | BSD-style terms in bundled notice | `licenses/jemalloc/copyright` |

Source and license references used for the two texts not present beside the supplied
binary bundle:

- FFmpeg 3.3.4: <https://github.com/FFmpeg/FFmpeg/tree/n3.3.4>
- OpenVR 1.14.15: <https://github.com/ValveSoftware/openvr/tree/v1.14.15>

The SHA256 and size of every runtime and notice file are fixed in
`runtime-manifest.json`. The jemalloc version is intentionally not guessed: the
supplied package reports unusable placeholder version metadata, while its bundled
copyright/license text is preserved verbatim.
