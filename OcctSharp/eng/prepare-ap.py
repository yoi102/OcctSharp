"""Freeze AP inputs and its exact source-audited private adapter catalogue."""
from pathlib import Path
import hashlib, json, shutil, urllib.request

w = Path(__file__).resolve().parents[1]
repo = w.parent
r = w / 'artifacts/batch-ap'
r.mkdir(exist_ok=True)
read = lambda p: json.loads(p.read_text(encoding='utf-8-sig'))
write = lambda p, v: p.write_text(json.dumps(v, indent=2) + '\n', encoding='utf-8')
sha = lambda p: hashlib.sha256(p.read_bytes()).hexdigest().upper()
b = r / 'preview39-baseline'
b.mkdir(exist_ok=True)
if not (b / 'manifest.json').exists():
    for src, dst in [('generated/manifest.json', 'manifest.json'),
                     ('artifacts/generator-reports/full-inventory.json', 'inventory.json'),
                     ('artifacts/batch-an/api-current.json', 'api.json'),
                     ('artifacts/native/Release/OcctSharp.Native.dll', 'OcctSharp.Native.dll'),
                     ('artifacts/batch-ao/delivery-result.json', 'delivery-result.json')]:
        shutil.copy2(w / src, b / dst)
    for f in read(b / 'manifest.json')['files']:
        p = f['relativePath']; q = b / 'generated' / p
        q.parent.mkdir(parents=True, exist_ok=True); shutil.copy2(w / p, q)

sdk = r / 'sdk'; sdk.mkdir(exist_ok=True)
commit = 'b8f597c677811d1f9f4d8a97f5ae2825c0353a42'
names = {'BRepLib_MakeEdge', 'BRepLib_MakeWire', 'BRepLib_Command',
         'ShapeAnalysis_ShapeContents', 'ShapeAnalysis_Wire'}
tree = read(w / 'artifacts/batch-am/sdk/tree.json')['tree']
identities = []
for x in tree:
    p = Path(x['path'])
    if p.stem not in names or p.suffix not in ('.hxx', '.cxx', '.lxx'): continue
    q = sdk / p.name
    if not q.exists():
        with urllib.request.urlopen(f'https://raw.githubusercontent.com/Open-Cascade-SAS/OCCT/{commit}/{x["path"]}') as response:
            q.write_bytes(response.read())
    identities.append({'path': x['path'], 'sha256': sha(q)})
write(sdk / 'source-identity.json', {'commit': commit, 'files': identities})

candidate = read(repo / 'docs/BATCH_AP_MODELING_CANDIDATES.json')
operations = []; audit = []
for d in candidate['candidates']:
    n = d['NativeName']; s = d['NativeSignature']; family = n.split('::')[0]
    admitted = False
    if family == 'ShapeAnalysis_ShapeContents':
        admitted = '&&' not in s and 'const ShapeAnalysis_ShapeContents &' not in s and 'ModifyOffest' not in n
        reason = 'Initialized private content analysis; explicit flags, copied counts and six owning shape groups.' if admitted else 'Implicit copy/move shares mutable sequence handles; deprecated alias is not a new useful operation.'
    elif family == 'BRepLib_MakeEdge':
        admitted = n.split('::')[-1] in ('Error', 'Edge', 'Vertex1', 'Vertex2') or s in (
            'BRepLib_MakeEdge::BRepLib_MakeEdge: void (const gp_Pnt &, const gp_Pnt &)',
            'BRepLib_MakeEdge::BRepLib_MakeEdge: void (const occ::handle<Geom_Curve> &, const double, const double)')
        reason = 'Call-private initialized builder; deep-copied curve; done/error separated; no unfinished result access.' if admitted else 'Outside the selected points/owned-curve workflow; default state, aliases and alternate constructors need independent initialization/lifetime audit.'
    elif family == 'BRepLib_MakeWire':
        admitted = n.split('::')[-1] in ('Error', 'Wire', 'Edge', 'Vertex') or s in (
            'BRepLib_MakeWire::BRepLib_MakeWire: void ()',
            'BRepLib_MakeWire::Add: void (const TopoDS_Edge &)')
        reason = 'Private builder receives one common geometry-copy graph; failure publishes diagnostics only, never partial wire.' if admitted else 'Alternate/implicit-copy constructors and mutable accumulation overloads are not needed for the closed ordered-edge workflow.'
    else:
        admitted = n.split('::')[-1] in ('CheckNotchedEdges', 'CheckDegenerated', 'CheckLacking', 'Init')
        reason = 'Exact face-member wire is copied together; bounded index, initialized coupled outputs and explicit native failure/status.' if admitted else 'Remaining curve handles, sequences, independent surfaces, implicit copies and RTTI are outside this private face-bound diagnostic profile.'
    item = dict(stableId=d['StableId'], nativeName=n, signature=s, header=d['Header'],
                baselineDisposition=d['State'], baselineReason=d['Category'],
                decision='ADMITTED' if admitted else 'DEFERRED', reason=reason)
    audit.append(item)
    if admitted: operations.append(dict(stableId=d['StableId'], nativeName=n, signature=s, guard='APPrivate', reason=reason))
all_rows = read(w / 'artifacts/generator-determinism/gp-pnt-discovery-1.json')['Model']['Declarations']
states = {d['StableId']: d for d in read(b / 'inventory.json')['FinalClassification']['Declarations']}
support = []
for d in all_rows:
    if d['NativeName'] != 'BRepLib_Command::IsDone': continue
    support.append(dict(stableId=d['StableId'], nativeName=d['NativeName'], signature=d['NativeSignature'], guard='APPrivate', reason='Done gate inherited by private Edge/Wire builders.'))
write(w / 'src/OcctSharp.Generator/Transformation/modeling-builders-operation-contracts.json', dict(schemaVersion='1.0', owners=[], operations=operations, supportingOperations=support))
write(repo / 'docs/BATCH_AP_MODELING_ADMISSION.json', dict(baseline='8.0.1-preview.39', state='AUDITED_IMPLEMENTING', inventorySha256=sha(b / 'inventory.json'), sdkSourceCommit=commit, sdkSourceIdentitySha256=sha(sdk / 'source-identity.json'), declarations=audit, supportingOperations=[dict(d, baselineDisposition=states[d['stableId']]['State']) for d in support], reused=candidate['reused']))
print(json.dumps({'admitted': len(operations), 'deferred': len(audit)-len(operations), 'support': len(support), 'sourceFiles': len(identities)}))
