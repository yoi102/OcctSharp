"""Freeze AR admission from delivered Preview.41, with scoped MeshData/Mesh rules."""
import json,hashlib
from pathlib import Path
from collections import Counter
w=Path(__file__).resolve().parents[1];r=w/'artifacts/batch-ar';docs=w.parent/'docs'
read=lambda p:json.loads(p.read_text(encoding='utf-8-sig'))
write=lambda p,v:p.write_text(json.dumps(v,indent=2)+'\n',encoding='utf-8')
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest().upper()
rows=read(r/'family-audit-input.json');candidates=[x for x in rows if x['State']=='Blocked']
assert len(candidates)==75
write(docs/'BATCH_AR_MESH_CANDIDATES.json',dict(schemaVersion='1.0',state='AUDIT_INPUT_NOT_ADMISSION',baseline='8.0.1-preview.41',inventorySha256=sha(r/'preview41-baseline/inventory.json'),families=dict(Counter(x['NativeName'].split('::')[0] for x in candidates)),candidates=candidates))
audit=[];data=[];meshing=[]
for x in candidates:
 f,m=x['NativeName'].split('::');s=x['NativeSignature'];admit=False;target='data'
 if f in {'Poly_ArrayOfNodes','Poly_ArrayOfUVNodes'}:
  pnt='gp_Pnt' if f=='Poly_ArrayOfNodes' else 'gp_Pnt2d'
  admit=s in {f'{f}::{f}: void ()',f'{f}::{f}: void (int)',f'{f}::IsDoublePrecision: bool () const',f'{f}::SetDoublePrecision: void (bool)',f'{f}::SetValue: void (int, const {pnt} &)',f'{f}::Value: {pnt} (int) const'}
 elif f=='Poly_Triangulation':admit=m in {'MapNodeArray','MapNormalArray','MapTriangleArray','MapUVNodeArray','SetNormals','MinMax','SetCachedMinMax'} or (m==f and 'const NCollection_Array1<gp_Pnt> &' in s)
 elif f=='Poly_Connect':admit=m in {'Load','Initialize','More','Next','Value','Triangle','Nodes'} or s=='Poly_Connect::Poly_Connect: void ()'
 elif f=='IMeshTools_Parameters':admit=s in {'IMeshTools_Parameters::IMeshTools_Parameters: void ()','IMeshTools_Parameters::IMeshTools_Parameters: void (const IMeshTools_Parameters &)'};target='meshing'
 elif f=='BRepMesh_DiscretRoot':admit=m=='SetShape';target='meshing'
 elif f=='BRepMesh_IncrementalMesh':admit=m in {'ChangeParameters','Parameters'} or s=='BRepMesh_IncrementalMesh::Perform: void (const Message_ProgressRange &)';target='meshing'
 reason=('Call-private initialized arrays, copied maps/bounds and bounded coherent connectivity; no native storage alias escapes.' if target=='data' else 'Private copied shape and parameter receiver; copy effective parameters and native flags; gate independently owned output.') if admit else 'Borrowed/mutable storage, pointer-array wrapping, move/copy receiver state, RTTI or external callbacks/context are outside this verified copied workflow.'
 op=dict(stableId=x['StableId'],nativeName=x['NativeName'],signature=s,guard='ARPrivate',reason=reason)
 audit.append(dict(op,header=x['Header'],baselineDisposition=x['State'],decision='ADMITTED' if admit else 'DEFERRED',workflow=target))
 if admit:(data if target=='data' else meshing).append(op)
found={x['StableId']:x for x in read(w/'artifacts/generator-determinism/gp-pnt-discovery-1.json')['Model']['Declarations']};states={x['StableId']:x for x in read(r/'preview41-baseline/inventory.json')['FinalClassification']['Declarations']}
support_data=[];support_mesh=[];reused=[];prerequisites=[]
for x in found.values():
 f,_,m=x['NativeName'].partition('::');s=x['NativeSignature'];target=None
 if f=='Poly_Triangulation' and (m in {'Copy','Deflection','SetMeshPurpose','MeshPurpose','HasUVNodes','HasNormals','NbNodes','NbTriangles','HasDeferredData','SetDoublePrecision','ResizeNodes','ResizeTriangles','SetNode','SetTriangle','AddUVNodes','SetUVNode'} or s=='Poly_Triangulation::Poly_Triangulation: void ()'):target='data'
 if f=='Poly_Triangle' and m in {'Poly_Triangle','Get'} and s in {'Poly_Triangle::Poly_Triangle: void (const int, const int, const int)','Poly_Triangle::Get: void (int &, int &, int &) const'}:target='data'
 if f=='Poly_Connect' and m=='Triangles':target='data'
 if f=='BRepMesh_IncrementalMesh' and m in {'GetStatusFlags','IsModified'} or s=='BRepMesh_IncrementalMesh::BRepMesh_IncrementalMesh: void ()':target='meshing'
 if f=='BRepMesh_DiscretRoot' and m=='IsDone':target='meshing'
 if f=='Message_ProgressRange' and s=='Message_ProgressRange::Message_ProgressRange: void ()':target='meshing'
 if f=='IMeshTools_Parameters' and m=='RelMinSize':target='meshing'
 if f=='BRepBuilderAPI_Copy' and s=='BRepBuilderAPI_Copy::BRepBuilderAPI_Copy: void (const TopoDS_Shape &, const bool, const bool)':target='meshing'
 if f=='BRep_Tool' and s in {'BRep_Tool::Triangulation: const occ::handle<Poly_Triangulation> &(const TopoDS_Face &, TopLoc_Location &, const Poly_MeshPurpose)','BRep_Tool::Surface: const occ::handle<Geom_Surface> &(const TopoDS_Face &, TopLoc_Location &)'}:target='meshing'
 if target is None:continue
 st=states[x['StableId']]['State'];assert st in {'Emitted','Manual','Blocked'},(s,st)
 if st=='Blocked':assert f=='Message_ProgressRange' and s=='Message_ProgressRange::Message_ProgressRange: void ()'
 op=dict(stableId=x['StableId'],nativeName=x['NativeName'],signature=s,guard='ARPrivate',reason='Verified already bound prerequisite; reused without counting new migration.')
 (support_data if target=='data' else support_mesh).append(op);(prerequisites if st=='Blocked' else reused).append(dict(op,workflow=target,baselineDisposition=st))
for resource,operations,support in [('poly-data-workspaces',data,support_data),('meshing-workspaces',meshing,support_mesh)]:write(w/f'src/OcctSharp.Generator/Transformation/{resource}-operation-contracts.json',dict(schemaVersion='1.0',owners=[],operations=operations,supportingOperations=support))
write(docs/'BATCH_AR_MESH_ADMISSION.json',dict(schemaVersion='1.0',state='AUDITED_IMPLEMENTING',baseline='8.0.1-preview.41',inventorySha256=sha(r/'preview41-baseline/inventory.json'),sdkSourceCommit=read(r/'sdk/source-identity.json')['commit'],sdkSourceIdentitySha256=sha(r/'sdk/source-identity.json'),declarations=audit,prerequisites=prerequisites,reused=reused))
print(dict(candidates=len(candidates),admitted=len(data)+len(meshing),deferred=sum(x['decision']=='DEFERRED' for x in audit),data=len(data),meshing=len(meshing),newPrerequisites=len(prerequisites),reused=len(reused)))
