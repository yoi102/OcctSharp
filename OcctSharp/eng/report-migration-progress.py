"""Join final declaration states to discovery modules; never use coverage.json totals."""

import argparse
from collections import Counter, defaultdict
from hashlib import sha256
import json
from pathlib import Path
import re


def read(path):
    return json.loads(path.read_text(encoding="utf-8-sig"))


def identity(path, root):
    return {"path": path.relative_to(root).as_posix(),
            "sha256": sha256(path.read_bytes()).hexdigest().upper()}


def unique(rows):
    result = {r["StableId"]: r for r in rows}
    if len(result) != len(rows):
        raise ValueError("Duplicate stable IDs")
    return result


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    for name in ("inventory", "discovery", "output"):
        parser.add_argument("--" + name, type=Path, required=True)
    parser.add_argument("--delivery", type=Path)
    parser.add_argument("--ap-candidates", type=Path)
    parser.add_argument("--version")
    parser.add_argument("--date")
    parser.add_argument("--extra-discovery", type=Path, action="append", default=[])
    args = parser.parse_args()
    root = Path(__file__).resolve().parents[2]
    inventory = read(args.inventory)
    discovery = read(args.discovery)
    delivery = read(args.delivery) if args.delivery else None
    if delivery is None and not (args.version and args.date):
        raise ValueError("Supply a frozen delivery or accounting version/date")
    rows = inventory["FinalClassification"]["Declarations"]
    unique(rows)
    found = unique(discovery["Model"]["Declarations"])
    for path in args.extra_discovery:
        supplemental = unique(read(path)["Model"]["Declarations"])
        for stable_id, row in supplemental.items():
            if stable_id in found and found[stable_id]["ProductModule"] != row["ProductModule"]:
                raise ValueError(f"Conflicting discovery module: {stable_id}")
            found.setdefault(stable_id, row)
    if delivery and identity(args.inventory.resolve(), root)["sha256"] != delivery["inventorySha256"]:
        raise ValueError("Inventory differs from frozen delivery")
    classifier = root / "OcctSharp/src/OcctSharp.Generator/Transformation/OcctProductModuleClassifier.cs"
    source = classifier.read_text(encoding="utf-8-sig")
    # Reviewed fallback for the private lexer declarations discovered by the full scan.
    if not re.search(r"OcctProductModule.Foundation,\s*\[.*?\"Expr\"", source, re.S):
        raise ValueError("Reviewed Expr package rule has changed")
    states = Counter()
    blocked = Counter()
    skipped = Counter()
    modules = defaultdict(Counter)
    reasons = defaultdict(Counter)
    families = defaultdict(Counter)
    fallback = []
    for row in rows:
        stable_id = row["StableId"]
        if stable_id in found:
            module = found[stable_id]["ProductModule"]
        elif (row["SourcePackage"] == "ExprIntrp"
              and row["Header"] == "ExprIntrp_yaccintrf.hxx"
              and row["State"] == "Blocked"
              and row["Category"] == "MissingToolkitProvenance"):
            module = "Foundation"
            fallback.append({"stableId": stable_id, "module": module,
                             "rule": "PackageRules: Expr prefix -> Foundation"})
        else:
            raise ValueError(f"Unreviewed module assignment: {stable_id}")
        if module in ("Unassigned", None):
            raise ValueError(f"Unassigned module: {stable_id}")
        states[row["State"]] += 1
        modules[module][row["State"]] += 1
        if row["State"] == "Blocked":
            blocked[row["Category"]] += 1
            reasons[module][row["Category"]] += 1
            families[module][row["NativeName"].split("::")[0]] += 1
        elif row["State"] == "Skipped":
            skipped[row["Category"]] += 1
    expected = {r["State"]: r["Count"] for r in inventory["FinalClassification"]["DeclarationStates"]}
    if delivery and expected != {r["State"]: r["Count"] for r in delivery["inventoryStates"]}:
        raise ValueError("Counts differ from frozen delivery")
    if dict(states) != expected or len(rows) != inventory["FinalClassification"]["DeclarationTotal"]:
        raise ValueError("Counts differ from final classification/delivery")
    bound = states["Emitted"] + states["Manual"]
    denominator = len(rows) - states["Skipped"]
    report = {
        "schemaVersion": "1.0", "state": "RECONCILED_ACCOUNTING_NOT_PRODUCT_COMPLETION",
        "date": delivery["date"] if delivery else args.date,
        "baseline": delivery["packageVersion"] if delivery else args.version,
        "frozenDeliveryVerified": delivery is not None,
        "inputs": [identity(p.resolve(), root) for p in
                   (args.inventory, args.discovery, *args.extra_discovery, *([args.delivery] if args.delivery else []), classifier, Path(__file__))],
        "total": len(rows), "states": dict(sorted(states.items())),
        "pending": inventory["FinalClassification"]["DeclarationPending"],
        "implementedDeclarations": bound, "nonSkippedDenominator": denominator,
        "allInventoryCoveragePercent": round(100 * bound / len(rows), 4),
        "nonSkippedCoveragePercent": round(100 * bound / denominator, 4),
        "semanticScan": inventory["SemanticScan"], "fullScanComplete": inventory["IsComplete"],
        "parserFailures": len(inventory["Failures"]),
        "finalClassificationComplete": inventory["FinalClassification"]["IsComplete"],
        "discoveryRows": len(found), "moduleFallbacks": fallback,
        "limitations": ["Declaration counts are not product-completion percentages.",
                        "Blocked includes intentionally deferred/internal APIs, not promised migration scope.",
                        "Skipped is not a remaining implementation queue.",
                        "Declarations undiscovered in failed headers have no known denominator.",
                        "Final Pending=0 does not mean the full semantic scan succeeded."],
        "blockedReasons": dict(blocked.most_common()),
        "skippedReasons": dict(skipped.most_common()), "modules": []}
    order = ["Foundation", "Geometry", "Modeling", "MeshData", "Mesh", "Documents",
             "DataExchange", "Xde", "Visualization", "IVtk", "Draw", "OpenGles"]
    if set(modules) - set(order):
        raise ValueError("Unreviewed product module")
    for module in order:
        if module not in modules:
            continue
        counts = {s: modules[module][s] for s in ("Emitted", "Manual", "Blocked", "Skipped")}
        ready = counts["Emitted"] + counts["Manual"]
        active = ready + counts["Blocked"]
        report["modules"].append({"module": module, "states": counts,
            "total": sum(counts.values()), "implemented": ready, "nonSkippedDenominator": active,
            "nonSkippedCoveragePercent": round(100 * ready / active, 2) if active else None,
            "blockedReasons": dict(reasons[module].most_common()),
            "topBlockedFamilies": dict(families[module].most_common(12))})
    ap_families = {"BRepLib_MakeEdge", "BRepLib_MakeWire",
                   "ShapeAnalysis_ShapeContents", "ShapeAnalysis_Wire"}
    selected = [dict(r, NativeSignature=found[r["StableId"]]["NativeSignature"],
                     ProductModule=found[r["StableId"]]["ProductModule"])
                for r in rows if r["NativeName"].split("::")[0] in ap_families]
    candidates = [r for r in selected if r["State"] == "Blocked"]
    reused = [r for r in selected if r["State"] in ("Emitted", "Manual")]
    ap = {"schemaVersion": "1.0", "state": "PLANNED_CANDIDATES_NOT_ADMITTED",
          "baseline": report["baseline"], "inputs": report["inputs"],
          "candidateCount": len(candidates),
          "families": dict(sorted(Counter(r["NativeName"].split("::")[0]
                                          for r in candidates).items())),
          "candidateModules": dict(sorted(Counter(r["ProductModule"] for r in candidates).items())),
          "reusedDispositions": dict(Counter(r["State"] for r in reused)),
          "candidates": sorted(candidates, key=lambda r: r["StableId"]),
          "reused": sorted(reused, key=lambda r: r["StableId"]),
          "notice": "Candidate counts are audit inputs, not admission or delivery promises."}
    outputs = [(args.output, report)]
    if args.ap_candidates:
        outputs.append((args.ap_candidates, ap))
    for path, data in outputs:
        path.write_text(json.dumps(data, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    print(json.dumps({"states": report["states"], "modules": len(report["modules"]),
                      "moduleFallbacks": len(fallback), "apCandidates": len(candidates),
                      "apReused": ap["reusedDispositions"]}))


if __name__ == "__main__":
    main()
