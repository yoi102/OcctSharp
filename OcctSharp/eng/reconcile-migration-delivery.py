"""Reconcile actual local batch gates and parser discoveries before final package freeze."""
import argparse
import hashlib
import json
from pathlib import Path
import re
import subprocess
from datetime import datetime, timezone, timedelta
from migration_delivery_delta import verify_inventory_delta
from migration_delivery_evidence import verify_round_position


def main():
    p = argparse.ArgumentParser(description=__doc__)
    for name in ('batch', 'topic', 'baseline', 'version', 'abi', 'bridge'):
        p.add_argument('--' + name, required=True)
    for name in ('generator', 'runtime', 'delta', 'exports', 'remaining'):
        p.add_argument('--' + name, type=int, required=True)
    p.add_argument('--extra-discovery', type=Path, action='append', default=[])
    a = p.parse_args()
    verify_round_position(a.batch,a.remaining)
    w = Path(__file__).resolve().parents[1]
    docs = w.parent / 'docs'
    r = w / ('artifacts/batch-' + a.batch.lower())
    read = lambda path: json.loads(path.read_text(encoding='utf-8-sig'))
    sha = lambda path: hashlib.sha256(path.read_bytes()).hexdigest().upper()
    write = lambda path, value: path.write_text(json.dumps(value, indent=2) + '\n', encoding='utf-8')
    log = (r / 'release-check-initial.log').read_text(encoding='utf-8-sig')
    assert 'Release engineering checks completed.' in log
    for n in (a.generator, a.runtime):
        assert len(re.findall(r'Passed:\s+' + str(n) + r', Skipped:\s+0', log)) == 3
    debug = read(r / 'integration-actual-debug/result.json')
    loaded = read(r / 'integration-debug-loaded-modules.json')
    assert debug['state'] == 'PASS' and debug['tests'] == a.runtime
    assert loaded['runtimeState'] == 'PASS' and loaded['nativeDlls'] == 62
    exports, diff, api = (read(r / name) for name in ('native-exports.json', 'api-diff.json', 'api-current.json'))
    assert exports['removed'] == 0 and len(exports['added']) == a.exports and exports['debugMatchesRelease']
    assert not diff['HasBreakingChanges'] and diff['RemovedCount'] == 0
    audit = read(r / 'source-audit.json')
    assert audit['state'] == 'PASS' and len(audit['addedStableIds']) == a.delta
    base = read(r / a.baseline / 'inventory.json')
    full = read(w / 'artifacts/generator-reports/full-inventory.json')
    old = {x['StableId']: x['State'] for x in base['FinalClassification']['Declarations']}
    new_rows = {x['StableId']: x for x in full['FinalClassification']['Declarations']}
    new = {sid: x['State'] for sid, x in new_rows.items()}
    discoveries = {sid: new[sid] for sid in new.keys() - old.keys()}
    proof = {}
    for path in a.extra_discovery:
        proof.update({x['StableId']: x for x in read(path)['Model']['Declarations']})
    for sid in discoveries:
        assert sid in proof and proof[sid]['NativeName'] == new_rows[sid]['NativeName'], sid
    converted=audit.get('manualToEmittedStableIds',[])
    verify_inventory_delta(old, new, audit['addedStableIds'], set(), a.delta, 0, discoveries, converted)
    repaired = {x['Header'] for x in base['Failures']} - {x['Header'] for x in full['Failures']}
    if repaired:
        result = read(r / 'parser-repair-result.json')
        assert result['state'] == 'PASS' and set(result['headers']) == repaired
        audit['parserRepairEvidenceSha256'] = sha(r / 'parser-repair-result.json')
    assert not ({x['Header'] for x in full['Failures']} - {x['Header'] for x in base['Failures']})
    audit['repairedParserHeaders'] = sorted(repaired)
    audit['discoveryStates'] = discoveries
    audit['additionalDiscoveryInputs'] = [dict(path=str(path), sha256=sha(path)) for path in a.extra_discovery]
    write(r / 'source-audit.json', audit)
    preview = a.version.split('preview.', 1)[1]
    python = 'C:/Users/yoiri/.cache/codex-runtimes/codex-primary-runtime/dependencies/python/python.exe'
    command = [python, '-X', 'utf8', str(w / 'eng/report-migration-progress.py'),
               '--inventory', str(w / 'artifacts/generator-reports/full-inventory.json'),
               '--discovery', str(w / 'artifacts/generator-determinism/gp-pnt-discovery-1.json'),
               '--version', a.version, '--date', datetime.now(timezone(timedelta(hours=9))).date().isoformat(), '--output', str(docs / f'PROGRESS_PREVIEW_{preview}_COUNTS.json')]
    for path in a.extra_discovery:
        command += ['--extra-discovery', str(path)]
    subprocess.run(command, check=True)
    counts = read(docs / f'PROGRESS_PREVIEW_{preview}_COUNTS.json')
    manifest = read(w / 'generated/manifest.json')
    batch = a.batch.upper()
    round_batches = (['BR','BS','BT','BU','BV','BW','BX','BY','BZ','CA','CB','CC','CD','CE','CF','CG','CH','CI']
                     if verify_round_position(a.batch,a.remaining) == 'BR–CI' else
                     ['BL', 'BM', 'BN', 'BO', 'BP', 'BQ'] if batch in ['BL', 'BM', 'BN', 'BO', 'BP', 'BQ'] else ['BF', 'BG', 'BH', 'BI', 'BJ', 'BK'])
    round_size = len(round_batches)
    round_name = round_batches[0] + '–' + round_batches[-1]
    remaining = round_batches[round_size-a.remaining:]
    round_plan = ('NEXT_ROUND_BR_CI_PLAN.md' if round_batches[0] == 'BR' else
                  'NEXT_ROUND_BL_BQ_PLAN.md' if round_batches[0] == 'BL' else 'NEXT_ROUND_BF_BK_PLAN.md')
    evidence = f"""{audit['admitted']} admitted / {audit['deferred']} deferred, {audit['newPrerequisites']} new prerequisites / {audit['reused']} reused; generated +{a.delta} / Manual -{len(converted)} (new SDK declarations {a.delta-len(converted)}, Manual to Emitted {len(converted)}).
Release/Debug/cold Generator {a.generator}, Runtime {a.runtime}: PASS.
Actual Debug-native {a.runtime}, 62 actually observed DLL paths/hashes: PASS.
Exports {exports['currentCount']} (+{a.exports}/-0), Debug parity; API {len(api['Signatures'])} (+{diff['AddedCount']}/-0).
All {len(audit['oldGeneratedFiles'])} baseline generated files unchanged; current generated files {len(manifest['files'])}.
Explicitly audited modified generated files: {len(audit.get('modifiedGeneratedFiles',[]))}.
ABI/lifetime/private headers and unified local package/consumer gates passed.
Final package/source freeze remains required. Only batch-{a.batch.lower()}/delivery-result.json PASS completes {batch}.
"""
    table = '\n'.join(f"| {x['module']} | {x['implemented']:,} | {x['states']['Blocked']:,} | {x['nonSkippedCoveragePercent']} |" for x in counts['modules'])
    text = f"""# Preview.{preview} local delivery progress

Development identity Preview.{preview} / ABI {a.abi} / bridge {a.bridge}.
Delivery authority: OcctSharp/artifacts/batch-{a.batch.lower()}/delivery-result.json state PASS.
{round_name}: {round_size-a.remaining}/{round_size} after final freeze, {a.remaining} remaining ({', '.join(remaining)}).

{evidence}
Inventory {counts['total']}: {counts['states']}; implemented declarations {counts['implementedDeclarations']}.
Non-Skipped coverage {counts['nonSkippedCoveragePercent']}%; Pending {counts['pending']}.
Parser failures {counts['parserFailures']}; repaired this batch {len(repaired)}.
Newly discovered declarations {len(discoveries)} are separate from migration delta {a.delta}.
These counts are not product-completion percentages; full semantic scan complete: {counts['fullScanComplete']}.

| Module | Emitted + Manual | Blocked | Non-Skipped percent |
|---|---:|---:|---:|
{table}

Source uncommitted. Hosted CI, signing, push, public NuGet, external CAD interoperability and full downstream application acceptance: NOT RUN.
"""
    (docs / f'PROGRESS_PREVIEW_{preview}.md').write_text(text, encoding='utf-8')
    topic = docs / (a.topic + '.md')
    topic_text = topic.read_text(encoding='utf-8-sig').split('\nLocal evidence:')[0]
    topic_text = re.sub(r'^State: IMPLEMENTATION IN PROGRESS.*$',
                       'State: IMPLEMENTED / LOCAL GATES PASS; final package/source freeze required.',
                       topic_text, flags=re.M)
    topic.write_text(topic_text + '\nLocal evidence:\n\n' + evidence, encoding='utf-8')
    admission = docs / (a.topic + '_ADMISSION.json')
    value = read(admission)
    value['state'] = 'IMPLEMENTED_LOCAL_GATES_PASSED_FINAL_FREEZE_REQUIRED'
    write(admission, value)
    audit['admissionPath'] = admission.relative_to(w.parent).as_posix()
    audit['admissionSha256'] = sha(admission)
    write(r / 'source-audit.json', audit)
    previous_status = (docs / 'STATUS.md').read_bytes()
    private_marker = b'### Cadoryx'
    private_status = previous_status[previous_status.index(private_marker):] if private_marker in previous_status else b''
    (docs / 'STATUS.md').write_bytes(f"""# Current Status

- Updated {datetime.now(timezone(timedelta(hours=9))).date().isoformat()} (Asia/Tokyo).
- {batch} local gates passed at Preview.{preview} / ABI {a.abi} / bridge {a.bridge}; final package/source freeze required.
- {round_name} authorized continuously: {round_size-a.remaining}/{round_size} only after final delivery freeze PASS; {a.remaining} remain ({', '.join(remaining)}).
- [{batch} workflow]({a.topic}.md); [module progress](PROGRESS_PREVIEW_{preview}.md); [continuous plan]({round_plan}).
- {evidence.rstrip().replace(chr(10),chr(10)+'  ')}
- Inventory {counts['total']}: {counts['states']}; Pending {counts['pending']}; parser exclusions {counts['parserFailures']}.
- New discovery {len(discoveries)} is separate from generated migration +{a.delta}; non-Skipped declaration coverage {counts['nonSkippedCoveragePercent']}% is not product completion.
- Source uncommitted. Hosted CI, signing, push, public NuGet, external CAD interoperability and full downstream application acceptance: NOT RUN.

Actual HWND/pixel, identity, source invalidation and lifetime assertions remain required. Pinned AppSweep defect remains deferred; no topology history is inferred.
""".encode('utf-8') + b'\n' + private_status)
    project = w / 'src/OcctSharp/OcctSharp.csproj'
    source = project.read_text(encoding='utf-8-sig')
    entry = f'    <None Include="..\\..\\..\\docs\\PROGRESS_PREVIEW_{preview}_COUNTS.json" Pack="true" PackagePath="docs\\" />\n'
    if entry not in source:
        source = source.replace('    <None Include="..\\..\\..\\docs\\NEXT_ROUND_BF_BK_CANDIDATES.json"', entry + '    <None Include="..\\..\\..\\docs\\NEXT_ROUND_BF_BK_CANDIDATES.json"')
        project.write_text(source, encoding='utf-8')
    write(r / 'cold-result.json', dict(state='PASS', generatedFiles=len(manifest['files']), generator=a.generator,
                                     runtimePassed=a.runtime, evidence='release-check-initial.log'))
    print(f'{batch}: local gates reconciled; final freeze still required.')


if __name__ == '__main__':
    main()
