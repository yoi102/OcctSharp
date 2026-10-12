"""Classify fixed-source parser exclusions, preserving unproven repairs explicitly."""
import argparse
import hashlib
import json
from pathlib import Path
import re
import subprocess


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--sdk-repo", type=Path, required=True)
    args = parser.parse_args()
    w = Path(__file__).resolve().parents[1]
    target = w / "artifacts/batch-bf"
    read = lambda p: json.loads(p.read_text(encoding="utf-8-sig"))
    sha = lambda p: hashlib.sha256(p.read_bytes()).hexdigest().upper()
    plan = read(w.parent / "docs/NEXT_ROUND_BF_BK_CANDIDATES.json")
    commit = plan["sdkSourceCommit"]
    paths = subprocess.check_output(["git", "-C", str(args.sdk_repo), "ls-tree", "-r", "--name-only", commit], encoding="utf-8").splitlines()
    by_name = {Path(p).name: p for p in paths}
    rows = []
    for entry in plan["parserAudit"]:
        header, error = entry["header"], entry["firstError"]
        missing = re.search(r"'([^']+)' file not found", error)
        dependency = missing[1] if missing else None
        source = by_name.get(header)
        if header.startswith("IVtk"):
            category, disposition = "OptionalVtkDependency", "DEFERRED"
        elif header.startswith("GeomBndLib_"):
            category, disposition = "DistributionMissingPinnedFragment", "REPAIR_REQUIRES_PARSE_EVIDENCE"
        elif header == "NCollection_Haft.h":
            category, disposition = "CppCliOnly", "EXCLUDED"
        elif header == "OpenGl_GLESExtensions.hxx":
            category, disposition = "GlesDesktopTypedefConflict", "DEFERRED"
        elif header == "RWGltf_GltfOStreamWriter.hxx":
            category, disposition = "OptionalRapidJsonIncludeClosure", "DEFERRED"
        else:
            category, disposition = "UnavailableLegacyDependency", "DEFERRED"
        identities = dict(header=header, firstError=error, category=category, disposition=disposition,
                          sourcePath=source, missingDependency=dependency,
                          missingDependencyPresentInPinnedSource=dependency in by_name if dependency else None)
        if source:
            data = subprocess.check_output(["git", "-C", str(args.sdk_repo), "show", commit + ":" + source])
            identities["sourceSha256"] = hashlib.sha256(data).hexdigest().upper()
        rows.append(identities)
    report = dict(state="AUDITED_REPAIRS_REQUIRE_RUNTIME_EVIDENCE", sdkSourceCommit=commit,
                  baselineInventorySha256=sha(target / "preview55-baseline/inventory.json"),
                  exclusions=rows)
    (target / "parser-source-audit.json").write_text(json.dumps(report, indent=2) + "\n", encoding="utf-8")
    configuration = read(w / "config/generation.json")
    config = dict(schemaVersion=configuration["schemaVersion"], occtVersion=configuration["occtVersion"],
                  scope="bf-geombndlib-parse-repair", headers=[r["header"] for r in rows if r["category"] == "DistributionMissingPinnedFragment"],
                  toolkitByPackage=configuration["toolkitByPackage"], supplementalIncludes=[dict(
                      path="sdk/GeomBndLib_InfiniteHelpers.pxx", sha256=sha(target / "sdk/GeomBndLib_InfiniteHelpers.pxx"))])
    (target / "parser-repair-config.json").write_text(json.dumps(config, indent=2) + "\n", encoding="utf-8")
    print(json.dumps(dict(audited=len(rows), repairHeaders=config["headers"])))


if __name__ == "__main__":
    main()
