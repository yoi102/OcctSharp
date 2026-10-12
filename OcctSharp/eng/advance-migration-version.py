"""Advance the active development identity without altering frozen deliveries."""
from pathlib import Path
import argparse
p=argparse.ArgumentParser()
p.add_argument('--from-preview',type=int,required=True)
p.add_argument('--from-abi',type=int,required=True)
p.add_argument('--from-bridge',type=int,required=True)
a=p.parse_args();w=Path(__file__).resolve().parents[1]
old=f'8.0.1-preview.{a.from_preview}';new=f'8.0.1-preview.{a.from_preview+1}'
files=['eng/pack.ps1','eng/release-check.ps1','eng/generate-release-metadata.ps1','eng/update-bundled-runtime.ps1','eng/update-release-checksums.ps1','eng/verify-package.ps1','eng/verify-bundled-runtime.ps1','Directory.Build.props','src/OcctSharp.Native/src/Runtime/Abi.cpp','tests/OcctSharp.Runtime.Tests/RuntimeTests.cs','tests/OcctSharp.PackageConsumer/Program.cs','samples/OcctSharp.Samples/Program.cs']
replacements=[(old,new),(f'1.{a.from_abi}',f'1.{a.from_abi+1}'),(f'0.{a.from_bridge}.0',f'0.{a.from_bridge+1}.0'),(f'new Version(1, {a.from_abi})',f'new Version(1, {a.from_abi+1})'),(f'0x{0x10000+a.from_abi:08X}U',f'0x{0x10001+a.from_abi:08X}U')]
assert old in (w/'Directory.Build.props').read_text()
for file in files:
 path=w/file;s=path.read_text(encoding='utf-8-sig')
 for before,after in replacements:s=s.replace(before,after)
 path.write_text(s,encoding='utf-8')
print(new)
