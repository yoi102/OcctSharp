"""Capture immutable migration inputs and fixed SDK sources without editing the SDK."""
import argparse
import hashlib
import json
from pathlib import Path
import shutil
import subprocess


def read(path):
    return json.loads(path.read_text(encoding="utf-8-sig"))


def sha(path):
    return hashlib.sha256(path.read_bytes()).hexdigest().upper()


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--batch", required=True)
    parser.add_argument("--baseline", required=True)
    parser.add_argument("--sdk-repo", type=Path, required=True)
    parser.add_argument("--extra", nargs="*", default=[])
    args = parser.parse_args()
    workspace = Path(__file__).resolve().parents[1]
    root = workspace.parent
    plan = read(root / "docs/NEXT_ROUND_BF_BK_CANDIDATES.json")
    scope = next(b for b in plan["batches"] if b["batch"] == args.batch.upper())
    target = workspace / "artifacts" / ("batch-" + args.batch.lower())
    baseline = target / ("preview" + args.baseline + "-baseline")
    if baseline.exists():
        raise FileExistsError(f"Refusing to overwrite existing baseline {baseline}")
    baseline.mkdir(parents=True)
    for src, dest in [
        ("generated/manifest.json", "manifest.json"),
        ("artifacts/generator-reports/full-inventory.json", "inventory.json"),
        ("artifacts/native/Release/OcctSharp.Native.dll", "OcctSharp.Native.dll"),
    ]:
        shutil.copy2(workspace / src, baseline / dest)
    manifest = read(baseline / "manifest.json")
    for entry in manifest["files"]:
        destination = baseline / "generated" / entry["relativePath"]
        destination.parent.mkdir(parents=True, exist_ok=True)
        shutil.copy2(workspace / entry["relativePath"], destination)
    subprocess.run([
        "dotnet", "run", "--project", str(workspace / "tools/OcctSharp.ApiTool"),
        "-c", "Release", "--no-build", "--", "snapshot",
        str(workspace / "src/OcctSharp/bin/Release/net10.0/win-x64/OcctSharp.dll"),
        str(baseline / "api.json")], check=True)
    commit = plan["sdkSourceCommit"]
    paths = subprocess.check_output([
        "git", "-C", str(args.sdk_repo), "ls-tree", "-r", "--name-only", commit
    ], encoding="utf-8").splitlines()
    names = {r["Header"] for r in scope["candidates"]} | set(args.extra)
    selected = [p for p in paths if Path(p).name in names
                or (Path(p).suffix == ".cxx" and Path(p).with_suffix(".hxx").name in names)]
    missing = names - {Path(p).name for p in selected}
    if missing:
        raise FileNotFoundError(f"Pinned SDK sources missing: {sorted(missing)}")
    sdk_root = Path(read(workspace / "config/local.settings.json")["occtRoot"])
    sdk_dir = target / "sdk"
    sdk_dir.mkdir()
    identities = []
    for path in selected:
        data = subprocess.check_output(["git", "-C", str(args.sdk_repo), "show", commit + ":" + path])
        dest = sdk_dir / Path(path).name
        if dest.exists():
            raise ValueError(f"Ambiguous SDK basename {dest.name}")
        dest.write_bytes(data)
        installed = sdk_root / "inc" / dest.name
        installed_hash = sha(installed) if installed.is_file() else None
        if dest.suffix == ".hxx" and installed_hash != sha(dest):
            raise ValueError(f"Installed header differs from pinned source: {dest.name}")
        identities.append(dict(path=path, sha256=sha(dest), installedSha256=installed_hash))
    (sdk_dir / "source-identity.json").write_text(json.dumps(dict(
        state="PASS", commit=commit, files=identities), indent=2) + "\n", encoding="utf-8")
    (target / "baseline-identity.json").write_text(json.dumps(dict(
        state="PASS", baseline=args.baseline, inventorySha256=sha(baseline / "inventory.json"),
        manifestSha256=sha(baseline / "manifest.json"), bridgeSha256=sha(baseline / "OcctSharp.Native.dll"),
        sdkSourceIdentitySha256=sha(sdk_dir / "source-identity.json")), indent=2) + "\n", encoding="utf-8")
    print(json.dumps(dict(batch=args.batch, candidates=scope["candidateCount"], sourceFiles=len(identities))))


if __name__ == "__main__":
    main()
