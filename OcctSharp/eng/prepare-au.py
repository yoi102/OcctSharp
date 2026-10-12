"""Exact parent-bound selection-volume admission against delivered Preview.44."""
import hashlib,json
from pathlib import Path
from collections import Counter
w=Path(__file__).resolve().parents[1];r=w/'artifacts/batch-au';docs=w.parent/'docs'
read=lambda p:json.loads(p.read_text(encoding='utf-8-sig'))
write=lambda p,x:p.write_text(json.dumps(x,indent=2)+'\n',encoding='utf-8')
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest().upper()
rows=read(r/'family-audit-input.json');primary=[d for d in rows if d['State']=='Blocked'];assert len(primary)==213
found={d['StableId']:d for d in read(w/'artifacts/generator-determinism/gp-pnt-discovery-1.json')['Model']['Declarations']}
states={d['StableId']:d for d in read(r/'preview44-baseline/inventory.json')['FinalClassification']['Declarations']}
volume={'ActiveVolume','InitPointSelectingVolume','InitBoxSelectingVolume','SetCamera','SetWindowSize','SetPixelTolerance','WindowSize','Camera','GetActiveSelectionType','AllowOverlapDetection','IsOverlapAllowed','IsScalableActiveVolume','GetVertices','GetPlanes','GetMousePosition','GetNearPickedPnt','GetFarPickedPnt','GetViewRayDirection','DetectedPoint','DistToGeometryCenter','OverlapsPoint','OverlapsBox','OverlapsSphere','OverlapsSegment','OverlapsTriangle','OverlapsPolygon'}
def choose(d):
 f,_,m=d['NativeName'].partition('::');s=d['NativeSignature']
 if f=='SelectMgr_SelectingVolumeManager':return m in volume or s in {'SelectMgr_SelectingVolumeManager::SelectMgr_SelectingVolumeManager: void ()','SelectMgr_SelectingVolumeManager::BuildSelectingVolume: void ()'}
 if f=='AIS_ViewController':return m in {'PickPoint','PickAxis','GravityPoint'} or s=='AIS_ViewController::AIS_ViewController: void ()'
 if f=='V3d_View':return m in {'DiagnosticInformation','BackgroundColor','GradientBackgroundColors'} or s=='V3d_View::StatisticInformation: void (NCollection_IndexedDataMap<TCollection_AsciiString, TCollection_AsciiString> &) const'
 return False
ops=[];audit=[]
for d in primary:
 admit=choose(d);x=found[d['StableId']];assert not admit or x['Access']=='Public'
 reason='Initialized private point/box frustum with copied camera, eight copied vertices/planes and bounded primitive probes; creating-thread viewer binding and exact conservative source stamp.' if d['NativeName'].startswith('SelectMgr') else 'Initialized call-private controller or copied existing-view diagnostics; no actors, callbacks or input buffers escape.'
 if not admit:reason='Custom intersectors, animation/touch/callback buffers, external mutable cameras/windows, borrowed resources, unproved transform/clipping combinations, redundant builders or repeated existing rendering features remain deferred.'
 item=dict(stableId=d['StableId'],nativeName=d['NativeName'],signature=d['NativeSignature'],guard='AUParentSelection',reason=reason)
 audit.append(dict(item,header=d['Header'],baselineDisposition=d['State'],decision='ADMITTED' if admit else 'DEFERRED'))
 if admit:ops.append(item)
support=[];pre=[];reused=[]
methods={'V3d_View':{'Camera','Window'},'Aspect_Window':{'Size'},'Graphic3d_Camera':{'ProjectionState','WorldViewState','IsStereo'},'AIS_InteractiveContext':{'PixelTolerance','ActivatedModes','IsDisplayed','MainSelector','UpdateCurrentViewer'},'AIS_Shape':{'Shape'},'SelectMgr_ViewerSelector':{'NbPicked','Picked'},'SelectMgr_EntityOwner':{'Selectable'},'SelectBasics_PickResult':{'IsValid','HasPickedPoint','Depth','PickedPoint'}}
ctors={'Graphic3d_Camera::Graphic3d_Camera: void (const occ::handle<Graphic3d_Camera> &)','SelectBasics_PickResult::SelectBasics_PickResult: void ()'}
for d in found.values():
 f,_,m=d['NativeName'].partition('::');s=d['NativeSignature']
 if not (m in methods.get(f,set()) or s in ctors):continue
 if d['Access']!='Public' or d['IsPureVirtual']:continue
 st=states[d['StableId']]['State'];assert st in {'Emitted','Manual','Blocked'},(s,st)
 item=dict(stableId=d['StableId'],nativeName=d['NativeName'],signature=s,guard='AUParentSelection',reason='Private source-stamp/selection/copy prerequisite; already bound declarations count only as reuse.')
 support.append(item);(pre if st=='Blocked' else reused).append(dict(item,baselineDisposition=st))
write(w/'src/OcctSharp.Generator/Transformation/selection-workspaces-operation-contracts.json',dict(schemaVersion='1.0',owners=[],operations=ops,supportingOperations=support))
write(docs/'BATCH_AU_SELECTION_WORKSPACES_CANDIDATES.json',dict(schemaVersion='1.0',state='AUDIT_INPUT_NOT_ADMISSION',baseline='8.0.1-preview.44',inventorySha256=sha(r/'preview44-baseline/inventory.json'),families=dict(Counter(d['NativeName'].split('::')[0] for d in primary)),candidates=primary))
write(docs/'BATCH_AU_SELECTION_WORKSPACES_ADMISSION.json',dict(schemaVersion='1.0',state='AUDITED_IMPLEMENTING',baseline='8.0.1-preview.44',inventorySha256=sha(r/'preview44-baseline/inventory.json'),sdkSourceCommit=read(r/'sdk/source-identity.json')['commit'],sdkSourceIdentitySha256=sha(r/'sdk/source-identity.json'),declarations=audit,prerequisites=pre,reused=reused))
print(dict(candidates=len(primary),admitted=len(ops),deferred=len(primary)-len(ops),newPrerequisites=len(pre),reused=len(reused),delta=len(ops)+len(pre)))
