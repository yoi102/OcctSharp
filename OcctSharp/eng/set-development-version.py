"""Advance local package/ABI identity and its exact runtime acceptance assertions together."""
import argparse
from pathlib import Path
import re


def replace_identity(source, old_version, new_version, old_abi, new_abi, old_bridge, new_bridge):
    old_major, old_minor = (int(x) for x in old_abi.split('.'))
    new_major, new_minor = (int(x) for x in new_abi.split('.'))
    if not (0 <= new_major <= 65535 and 0 <= new_minor <= 65535):
        raise ValueError('ABI components exceed uint16.')
    def token(text, old, new):
        return re.sub(r'(?<![\d.])' + re.escape(old) + r'(?![\d.])', lambda _: new, text)
    return (token(token(token(source, old_version, new_version), old_bridge, new_bridge), old_abi, new_abi)
            .replace(f'new Version({old_major}, {old_minor})', f'new Version({new_major}, {new_minor})')
            .replace(f'0x{(old_major << 16) | old_minor:08X}U', f'0x{(new_major << 16) | new_minor:08X}U'))


def main():
    p = argparse.ArgumentParser(description=__doc__)
    p.add_argument('--preview', type=int, required=True)
    p.add_argument('--abi', required=True)
    p.add_argument('--bridge', required=True)
    a = p.parse_args()
    w = Path(__file__).resolve().parents[1]
    props = w / 'Directory.Build.props'
    native = w / 'src/OcctSharp.Native/src/Runtime/Abi.cpp'
    old_version = re.search(r'>8\.0\.1-preview\.\d+<', props.read_text(encoding='utf-8-sig'))[0][1:-1]
    source = native.read_text(encoding='utf-8-sig')
    encoded = int(re.search(r'AbiVersion = 0x([0-9A-F]+)U', source)[1], 16)
    old_abi = f'{encoded >> 16}.{encoded & 65535}'
    old_bridge = re.search(r'BridgeVersion = "([^"]+)"', source)[1]
    new_version = f'8.0.1-preview.{a.preview}'
    if a.preview <= int(old_version.rsplit('.', 1)[1]):
        raise ValueError('A new delivery needs a strictly increasing immutable package version.')
    files = [props, native, w / 'tests/OcctSharp.Runtime.Tests/RuntimeTests.cs',
             w / 'samples/OcctSharp.Samples/Program.cs', w / 'tests/OcctSharp.PackageConsumer/Program.cs',
             *sorted((w / 'eng').glob('*.ps1'))]
    changed = []
    for path in files:
        before = path.read_text(encoding='utf-8-sig')
        after = replace_identity(before, old_version, new_version, old_abi, a.abi, old_bridge, a.bridge)
        if after != before:
            path.write_text(after, encoding='utf-8')
            changed.append(str(path.relative_to(w)))
    print(f'{old_version}/{old_abi}/{old_bridge} -> {new_version}/{a.abi}/{a.bridge}: {len(changed)} source files.')


if __name__ == '__main__':
    main()
