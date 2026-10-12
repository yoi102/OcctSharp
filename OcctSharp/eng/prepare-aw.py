"""Freeze AW exact private spline/filling and controlled assembly scope after AV."""
from pathlib import Path
import json,hashlib,shutil
w=Path(__file__).resolve().parents[1];docs=w.parent/'docs';r=w/'artifacts/batch-aw'
read=lambda p:json.loads(p.read_text(encoding='utf-8-sig'))
write=lambda p,v:p.write_text(json.dumps(v,indent=2)+'\n',encoding='utf-8')
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest().upper()
assert read(w/'artifacts/batch-av/delivery-result.json')['state']=='PASS'
b=r/'preview46-baseline';b.mkdir(exist_ok=True)
if not (b/'manifest.json').exists():
 for source,name in [('generated/manifest.json','manifest.json'),('artifacts/generator-reports/full-inventory.json','inventory.json'),('artifacts/batch-av/api-current.json','api.json'),('artifacts/native/Release/OcctSharp.Native.dll','OcctSharp.Native.dll'),('artifacts/batch-av/delivery-result.json','delivery-result.json')]:shutil.copy2(w/source,b/name)
 for f in read(b/'manifest.json')['files']:
  target=b/'generated'/f['relativePath'];target.parent.mkdir(parents=True,exist_ok=True);shutil.copy2(w/f['relativePath'],target)
states={x['StableId']:x['State'] for x in read(b/'inventory.json')['FinalClassification']['Declarations']}
primary=read(r/'preliminary-audit-input.json')['declarations'];selected=[];deferred=[]
fillNames={'Init','SetContinuity','Continuity','SetParType','ParType','CriteriumWeight','SetCriteriumWeight','IsDone','Perform','PerformSmoothing','Surface','SurfShape','SurfPoles','SurfWeights','SurfUKnots','SurfVKnots','SurfUMults','SurfVMults','UDegree','VDegree','TolReached'}
builderSignatures={
'BRep_Builder::MakeVertex: void (TopoDS_Vertex &, const gp_Pnt &, const double) const',
'BRep_Builder::MakeVertex: void (TopoDS_Vertex &) const',
'BRep_Builder::UpdateVertex: void (const TopoDS_Vertex &, const gp_Pnt &, const double) const',
'BRep_Builder::UpdateVertex: void (const TopoDS_Vertex &, const double) const',
'BRep_Builder::UpdateVertex: void (const TopoDS_Vertex &, const double, const TopoDS_Edge &, const double) const',
'BRep_Builder::MakeEdge: void (TopoDS_Edge &, const occ::handle<Geom_Curve> &, const double) const',
'BRep_Builder::Range: void (const TopoDS_Edge &, const double, const double, const bool) const',
'BRep_Builder::SameParameter: void (const TopoDS_Edge &, const bool) const',
'BRep_Builder::SameRange: void (const TopoDS_Edge &, const bool) const',
'BRep_Builder::UpdateEdge: void (const TopoDS_Edge &, const double) const'}
def choose(d):
 f,_,m=d['NativeName'].partition('::');s=d['NativeSignature']
 if f=='BSplCLib':return m in {'D0','D1','D2','D3'} and 'NCollection_Array1<' in s and ('const int, const int, const bool' in s or 'const double, const NCollection_Array1<gp_Pnt' in s)
 if f=='GeomFill_AppSweep':return False # upstream section transform assignment defect; no unsafe receiver admitted
 if f=='GeomFill_AppSurf':return m in fillNames or m==f and s.endswith('void ()')
 return s in builderSignatures
for d in primary:
 if states[d['StableId']]!='Blocked':continue
 admitted=choose(d) and d['Access']=='Public' and not d.get('IsTemplated') and not d.get('IsPureVirtual')
 item=dict(stableId=d['StableId'],nativeName=d['NativeName'],signature=d['NativeSignature'],header=d['Header'],baselineDisposition='Blocked',decision='ADMITTED' if admitted else 'DEFERRED',guard='AWPrivateGeometry' if d['NativeName'].split('::')[0]!='BRep_Builder' else 'AWPrivateAssembly',reason='Private initialized bounded arrays/receivers, copied inputs, completion-gated surface and independent topology; no guessed topology history.' if admitted else 'Unsafe raw scratch buffers, callback/global ownership, unproved representations, borrowed mutable topology, redundant overload or unsupported 2D filling channels remain deferred.')
 if d['NativeName'].startswith('GeomFill_AppSweep::'):item['reason']='Upstream SweepSectionGenerator transform assignment is absent on ordinary tangent branches. Actual straight-path sweep fails three modes and the completing mode degenerates; receiver deferred. Bounded private circular sections use AppSurf instead.'
 (selected if admitted else deferred).append(item)
found=read(w/'artifacts/generator-determinism/gp-pnt-discovery-1.json')['Model']['Declarations']
supportNames={'GeomFill_Line':{'GeomFill_Line','NbPoints'},'GeomFill_SectionGenerator':{'GeomFill_SectionGenerator','SetParam'},'GeomFill_Profiler':{'AddCurve','Perform'},'Geom_BSplineCurve':{'Copy','D1','Poles','Weights','Knots','Multiplicities','Degree','IsPeriodic','FirstParameter','LastParameter','NbPoles','NbKnots'},'Geom_BSplineSurface':{'Geom_BSplineSurface','Copy'},'BRepCheck_Analyzer':{'BRepCheck_Analyzer','IsValid'},'TopoDS_Builder':{'Add'},'Geom_Circle':{'Geom_Circle'},'Geom_TrimmedCurve':{'Geom_TrimmedCurve'},'GeomConvert':{'CurveToBSplineCurve'}}
support=[];new=[];reused=[];primaryIds={x['stableId'] for x in selected}
for d in found:
 f,_,m=d['NativeName'].partition('::');s=d['NativeSignature']
 if m not in supportNames.get(f,set()) or d['Access']!='Public' or d['StableId'] in primaryIds or states[d['StableId']]=='Skipped' or '&&' in s:continue
 if f=='GeomFill_Line' and m==f and s!='GeomFill_Line::GeomFill_Line: void (const int)':continue
 if f=='Geom_Circle' and s!='Geom_Circle::Geom_Circle: void (const gp_Ax2 &, const double)':continue
 if f=='Geom_TrimmedCurve' and s!='Geom_TrimmedCurve::Geom_TrimmedCurve: void (const occ::handle<Geom_Curve> &, const double, const double, const bool, const bool)':continue
 if f=='GeomConvert' and s!='GeomConvert::CurveToBSplineCurve: occ::handle<Geom_BSplineCurve> (const occ::handle<Geom_Curve> &, const Convert_ParameterisationType)':continue
 if f=='GeomFill_SweepSectionGenerator' and m in {f,'Init'} and s not in {'GeomFill_SweepSectionGenerator::GeomFill_SweepSectionGenerator: void ()','GeomFill_SweepSectionGenerator::Init: void (const occ::handle<Geom_Curve> &, const double)'}:continue
 if f=='Geom_BSplineCurve' and m in {'Poles','Weights','Knots','Multiplicities'} and not s.endswith('() const'):continue
 if f=='Geom_BSplineSurface' and m==f and 'Array2<double>' not in s:continue
 if f=='BRepCheck_Analyzer' and m==f and 'TopoDS_Shape' not in s:continue
 if f=='BRepCheck_Analyzer' and m=='IsValid' and '() const' not in s:continue
 item=dict(stableId=d['StableId'],nativeName=d['NativeName'],signature=s,guard='AWPrivateAssembly' if f in {'BRepCheck_Analyzer','TopoDS_Builder'} else 'AWPrivateGeometry',reason='Private copied workflow prerequisite; no borrowed output escapes.')
 support.append(item);(new if states[d['StableId']]=='Blocked' else reused).append(dict(item,baselineDisposition=states[d['StableId']]))
for guard,resource in [('AWPrivateGeometry','filling-workspaces'),('AWPrivateAssembly','topology-assembly')]:
 write(w/('src/OcctSharp.Generator/Transformation/'+resource+'-operation-contracts.json'),dict(schemaVersion='1.0',owners=[],operations=[{k:x[k] for k in ['stableId','nativeName','signature','guard','reason']} for x in selected if x['guard']==guard],supportingOperations=[x for x in support if x['guard']==guard]))
write(docs/'BATCH_AW_GEOMETRY_MODELING_CANDIDATES.json',dict(state='AUDIT_INPUT_NOT_ADMISSION',baseline='8.0.1-preview.46',inventorySha256=sha(b/'inventory.json'),candidates=[d for d in primary if states[d['StableId']]=='Blocked']))
write(docs/'BATCH_AW_GEOMETRY_MODELING_ADMISSION.json',dict(state='AUDITED_IMPLEMENTING',baseline='8.0.1-preview.46',inventorySha256=sha(b/'inventory.json'),sdkSourceCommit=read(r/'sdk/source-identity.json')['commit'],sdkSourceIdentitySha256=sha(r/'sdk/source-identity.json'),declarations=selected+deferred,prerequisites=new,reused=reused))
print(dict(primary=len(selected)+len(deferred),admitted=len(selected),deferred=len(deferred),newPrerequisites=len(new),reused=len(reused),generatedDelta=len(selected)+len(new)))
