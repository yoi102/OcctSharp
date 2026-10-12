"""Freeze the delivered AP baseline and exact curved-hatching workflow admission."""
from pathlib import Path
import hashlib,json,shutil,subprocess
from collections import Counter

w=Path(__file__).resolve().parents[1];repo=w.parent;r=w/'artifacts/batch-aq';r.mkdir(exist_ok=True)
read=lambda p:json.loads(p.read_text(encoding='utf-8-sig'))
write=lambda p,v:p.write_text(json.dumps(v,indent=2)+'\n',encoding='utf-8')
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest().upper()
b=r/'preview40-baseline';b.mkdir(exist_ok=True)
if not (b/'manifest.json').exists():
 delivery=read(w/'artifacts/batch-ap/delivery-result.json');assert delivery['state']=='PASS' and delivery['packageVersion']=='8.0.1-preview.40'
 for src,dst in [('generated/manifest.json','manifest.json'),('artifacts/generator-reports/full-inventory.json','inventory.json'),('artifacts/batch-ap/api-current.json','api.json'),('artifacts/native/Release/OcctSharp.Native.dll','OcctSharp.Native.dll'),('artifacts/batch-ap/delivery-result.json','delivery-result.json')]:shutil.copy2(w/src,b/dst)
 for f in read(b/'manifest.json')['files']:
  p=f['relativePath'];q=b/'generated'/p;q.parent.mkdir(parents=True,exist_ok=True);shutil.copy2(w/p,q)
assert sha(b/'inventory.json')==read(b/'delivery-result.json')['inventorySha256']
inventory=read(b/'inventory.json');discovery=read(w/'artifacts/generator-determinism/gp-pnt-discovery-1.json')
found={d['StableId']:d for d in discovery['Model']['Declarations']};states={d['StableId']:d for d in inventory['FinalClassification']['Declarations']}
families={'Geom2dHatch_Hatcher','Geom2dHatch_Intersector'}
candidates=[dict(d,NativeSignature=found[d['StableId']]['NativeSignature'],ProductModule=found[d['StableId']]['ProductModule']) for d in states.values() if d['NativeName'].split('::')[0] in families and d['State']=='Blocked']
write(repo/'docs/BATCH_AQ_HATCHING_CANDIDATES.json',dict(schemaVersion='1.0',state='AUDIT_INPUT_NOT_ADMISSION',baseline='8.0.1-preview.40',inventorySha256=sha(b/'inventory.json'),families=dict(Counter(d['NativeName'].split('::')[0] for d in candidates)),candidates=candidates))
sdk=r/'sdk';sdk.mkdir(exist_ok=True);commit='b8f597c677811d1f9f4d8a97f5ae2825c0353a42'
names=families|{'Geom2dHatch_Hatching','Geom2dHatch_Element','HatchGen_Domain','HatchGen_PointOnHatching','HatchGen_PointOnElement','HatchGen_IntersectionPoint','Geom2dAdaptor_Curve','Geom2d_TrimmedCurve','GeomAPI'}
tree=read(w/'artifacts/batch-am/sdk/tree.json')['tree'];identities=[]
for x in tree:
 p=Path(x['path'])
 if p.stem not in names or p.suffix not in ('.hxx','.lxx','.cxx'):continue
 q=sdk/p.name
 data=subprocess.check_output(['git','-C','C:/Users/yoiri/source/repos/OCCT','show',commit+':'+x['path']]);q.write_bytes(data)
 identities.append(dict(path=x['path'],sha256=sha(q)))
write(sdk/'source-identity.json',dict(commit=commit,files=identities))
operations=[];audit=[]
for d in candidates:
 n=d['NativeName'];s=d['NativeSignature'];f,m=n.split('::')
 if f=='Geom2dHatch_Hatcher':
  admitted=m in {'AddHatching','Domain','NbDomains','NbPoints','Point','Status','TrimDone','TrimFailed'} or s in {
   'Geom2dHatch_Hatcher::IsDone: bool (const int) const',
   'Geom2dHatch_Hatcher::Geom2dHatch_Hatcher: void (const Geom2dHatch_Intersector &, const double, const double, const bool, const bool)',
   'Geom2dHatch_Hatcher::AddElement: int (const Geom2dAdaptor_Curve &, const TopAbs_Orientation)',
   'Geom2dHatch_Hatcher::Trim: void ()','Geom2dHatch_Hatcher::ComputeDomains: void ()',
   'Geom2dHatch_Hatcher::Confusion2d: double () const','Geom2dHatch_Hatcher::Confusion3d: double () const',
   'Geom2dHatch_Hatcher::KeepPoints: bool () const','Geom2dHatch_Hatcher::KeepSegments: bool () const'}
 else:
  admitted=m in {'ConfusionTolerance','TangencyTolerance'} or s=='Geom2dHatch_Intersector::Geom2dHatch_Intersector: void (const double, const double)'
 reason='Initialized call-private curved hatching; copied inputs, gated statuses/events/domains and independent trimmed curves.' if admitted else 'Mutable receiver/borrowed intersector/adaptor, implicit aliases, alternate paths or debug dump outside the frozen batch workflow.'
 if s=='Geom2dHatch_Hatcher::IsDone: bool () const':reason='Pinned header declares the no-argument IsDone, but pinned implementation and import library provide no definition; native link fails. Aggregate the verified per-hatching overload without claiming this declaration emitted.'
 item=dict(stableId=d['StableId'],nativeName=n,signature=s,header=d['Header'],baselineDisposition=d['State'],decision='ADMITTED' if admitted else 'DEFERRED',reason=reason);audit.append(item)
 if admitted:operations.append(dict(stableId=d['StableId'],nativeName=n,signature=s,guard='AQPrivate',reason=reason))
support=[];reused=[];prerequisites=[]
for d in found.values():
 f,_,m=d['NativeName'].partition('::');s=d['NativeSignature']
 selected=(f=='HatchGen_IntersectionPoint' and m in {'Index','Parameter','Position','StateBefore','StateAfter','SegmentBeginning','SegmentEnd'}) or (f=='HatchGen_Domain' and m in {'HasFirstPoint','HasSecondPoint','FirstPoint','SecondPoint'}) or (f=='HatchGen_PointOnHatching' and m in {'NbPoints','Point'}) or (f=='HatchGen_PointOnElement' and m=='IntersectionType') or s in {
  'Geom2dAdaptor_Curve::Geom2dAdaptor_Curve: void (const occ::handle<Geom2d_Curve> &, const double, const double)',
  'Geom2d_TrimmedCurve::Geom2d_TrimmedCurve: void (const occ::handle<Geom2d_Curve> &, const double, const double, const bool, const bool)',
  'GeomAPI::To3d: occ::handle<Geom_Curve> (const occ::handle<Geom2d_Curve> &, const gp_Pln &)'}
 if not selected:continue
 baseline=states[d['StableId']]['State'];assert baseline in ('Emitted','Manual','Blocked')
 item=dict(stableId=d['StableId'],nativeName=d['NativeName'],signature=s,guard='AQPrivate',reason='Necessary copied event/domain/curve prerequisite; preserve existing emitted/manual public surface.',baselineDisposition=baseline)
 (prerequisites if baseline=='Blocked' else reused).append(item)
 support.append({k:v for k,v in item.items() if k!='baselineDisposition'})
write(w/'src/OcctSharp.Generator/Transformation/hatching-workspaces-operation-contracts.json',dict(schemaVersion='1.0',owners=[],operations=operations,supportingOperations=support))
write(repo/'docs/BATCH_AQ_HATCHING_ADMISSION.json',dict(schemaVersion='1.0',state='AUDITED_IMPLEMENTING',baseline='8.0.1-preview.40',inventorySha256=sha(b/'inventory.json'),sdkSourceCommit=commit,sdkSourceIdentitySha256=sha(sdk/'source-identity.json'),declarations=audit,prerequisites=prerequisites,reused=reused))
print(json.dumps(dict(candidates=len(candidates),admitted=len(operations),deferred=len(audit)-len(operations),newPrerequisites=len(prerequisites),reused=len(reused),sourceFiles=len(identities))))
