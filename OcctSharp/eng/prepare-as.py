"""Exact AS admission over delivered Preview.42; no global mutable-container mapping."""
import hashlib,json,subprocess
from pathlib import Path
from collections import Counter
w=Path(__file__).resolve().parents[1];r=w/'artifacts/batch-as';docs=w.parent/'docs'
read=lambda p:json.loads(p.read_text(encoding='utf-8-sig'))
write=lambda p,x:p.write_text(json.dumps(x,indent=2)+'\n',encoding='utf-8')
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest().upper()
rows=read(r/'family-audit-input.json');primary=[d for d in rows if d['State']=='Blocked'];assert len(primary)==163
found={d['StableId']:d for d in read(w/'artifacts/generator-determinism/gp-pnt-discovery-1.json')['Model']['Declarations']}
states={d['StableId']:d for d in read(r/'preview42-baseline/inventory.json')['FinalClassification']['Declarations']}
def choose(d):
 f,_,m=d['NativeName'].partition('::');s=d['NativeSignature']
 if f=='TDataStd_NamedData':return m in {'ChangeIntegers','ChangeReals','ChangeStrings','ChangeBytes','ChangeArraysOfIntegers','ChangeArraysOfReals','GetIntegersContainer','GetRealsContainer','GetStringsContainer','GetBytesContainer','GetArraysOfIntegersContainer','GetArraysOfRealsContainer'}
 if f=='TDF_Tool':return s in {'TDF_Tool::Entry: void (const TDF_Label &, TCollection_AsciiString &)','TDF_Tool::NbLabels: int (const TDF_Label &)','TDF_Tool::NbAttributes: int (const TDF_Label &)','TDF_Tool::IsSelfContained: bool (const TDF_Label &)','TDF_Tool::TagList: void (const TDF_Label &, NCollection_List<int> &)','TDF_Tool::Label: void (const occ::handle<TDF_Data> &, const char *const, TDF_Label &, const bool)'}
 if f=='XCAFDoc_LayerTool':return s in {
  'XCAFDoc_LayerTool::AddLayer: TDF_Label (const TCollection_ExtendedString &) const',
  'XCAFDoc_LayerTool::FindLayer: bool (const TCollection_ExtendedString &, TDF_Label &) const',
  'XCAFDoc_LayerTool::GetLayer: bool (const TDF_Label &, TCollection_ExtendedString &) const',
  'XCAFDoc_LayerTool::GetLayerLabels: void (NCollection_Sequence<TDF_Label> &) const',
  'XCAFDoc_LayerTool::GetShapesOfLayer: void (const TDF_Label &, NCollection_Sequence<TDF_Label> &)',
  'XCAFDoc_LayerTool::IsLayer: bool (const TDF_Label &) const',
  'XCAFDoc_LayerTool::IsVisible: bool (const TDF_Label &) const',
  'XCAFDoc_LayerTool::SetVisibility: void (const TDF_Label &, const bool) const',
  'XCAFDoc_LayerTool::SetLayer: void (const TDF_Label &, const TDF_Label &, const bool) const',
  'XCAFDoc_LayerTool::UnSetOneLayer: bool (const TDF_Label &, const TDF_Label &) const',
  'XCAFDoc_LayerTool::RemoveLayer: void (const TDF_Label &) const'}
 if f=='XCAFDoc_NotesTool':return s in {
  'XCAFDoc_NotesTool::CreateComment: occ::handle<XCAFDoc_Note> (const TCollection_ExtendedString &, const TCollection_ExtendedString &, const TCollection_ExtendedString &)',
  'XCAFDoc_NotesTool::CreateBalloon: occ::handle<XCAFDoc_Note> (const TCollection_ExtendedString &, const TCollection_ExtendedString &, const TCollection_ExtendedString &)',
  'XCAFDoc_NotesTool::GetNotes: void (NCollection_Sequence<TDF_Label> &) const',
  'XCAFDoc_NotesTool::GetNotes: int (const TDF_Label &, NCollection_Sequence<TDF_Label> &) const',
  'XCAFDoc_NotesTool::GetNotes: int (const XCAFDoc_AssemblyItemId &, NCollection_Sequence<TDF_Label> &) const',
  'XCAFDoc_NotesTool::GetAttrNotes: int (const XCAFDoc_AssemblyItemId &, const Standard_GUID &, NCollection_Sequence<TDF_Label> &) const',
  'XCAFDoc_NotesTool::GetSubshapeNotes: int (const XCAFDoc_AssemblyItemId &, int, NCollection_Sequence<TDF_Label> &) const',
  'XCAFDoc_NotesTool::GetAnnotatedItems: void (NCollection_Sequence<TDF_Label> &) const',
  'XCAFDoc_NotesTool::FindAnnotatedItem: TDF_Label (const TDF_Label &) const',
  'XCAFDoc_NotesTool::IsAnnotatedItem: bool (const TDF_Label &) const',
  'XCAFDoc_NotesTool::AddNote: occ::handle<XCAFDoc_AssemblyItemRef> (const TDF_Label &, const TDF_Label &)',
  'XCAFDoc_NotesTool::RemoveNote: bool (const TDF_Label &, const TDF_Label &, bool)',
  'XCAFDoc_NotesTool::DeleteNote: bool (const TDF_Label &)'}
 if f=='XCAFDoc_Note':return m in {'Get','IsMine','UserName','TimeStamp'}
 if f=='XCAFDoc_NoteComment':return m in {'Get','Comment'}
 if f=='XCAFDoc_NoteBalloon':return m=='Get'
 return False
audit=[];operations={'document-projections':[],'xde-attribute-projections':[]}
for d in primary:
 x=found[d['StableId']];admit=choose(d);assert not admit or x['Access']=='Public',d
 target='document-projections' if d['NativeName'].split('::')[0] in {'TDataStd_NamedData','TDF_Tool'} else 'xde-attribute-projections'
 reason=('Scoped document receiver, private containers and nested atomic replacement; only detached typed maps/label diagnostics escape.' if target=='document-projections' else 'Document-bound layer/note operations, validated same-document labels and detached table/association/Unicode projections.') if admit else ('Streams, RTTI, inaccessible mutators, unrestricted relocation/filters, external objects, binary note IO or redundant overloads are outside the verified copied workflow.')
 op=dict(stableId=d['StableId'],nativeName=d['NativeName'],signature=d['NativeSignature'],guard='ASDocumentBound',reason=reason)
 audit.append(dict(op,header=d['Header'],baselineDisposition=d['State'],decision='ADMITTED' if admit else 'DEFERRED',workflow=target))
 if admit:operations[target].append(op)
support={'document-projections':[],'xde-attribute-projections':[]};prerequisites=[];reused=[]
for d in found.values():
 f,_,m=d['NativeName'].partition('::');s=d['NativeSignature'];target=None
 if f=='TDataStd_NamedData' and (m=='GetID' or s=='TDataStd_NamedData::TDataStd_NamedData: void ()'):target='document-projections'
 if f=='TDF_Label' and s in {'TDF_Label::AddAttribute: void (const occ::handle<TDF_Attribute> &, const bool) const','TDF_Label::ForgetAttribute: bool (const Standard_GUID &) const'}:target='document-projections'
 if f=='TDocStd_Document' and (m in {'GetData','HasOpenCommand','SetUndoLimit','Main'} or s=='TDocStd_Document::TDocStd_Document: void (const TCollection_ExtendedString &)'):target='document-projections'
 if f=='TDF_Transaction' and (m in {'Open','Commit','Abort'} or s=='TDF_Transaction::TDF_Transaction: void (const occ::handle<TDF_Data> &, const TCollection_AsciiString &)'):target='document-projections'
 if f=='XCAFDoc_DocumentTool' and (m in {'LayerTool','NotesTool','CheckLayerTool','CheckNotesTool'} or s=='XCAFDoc_DocumentTool::Set: occ::handle<XCAFDoc_DocumentTool> (const TDF_Label &, const bool)'):target='xde-attribute-projections'
 if f=='XCAFDoc_NotesTool' and m in {'NbNotes','NbAnnotatedItems'} or f=='XCAFDoc_Note' and m=='IsOrphan':target='xde-attribute-projections'
 if f=='XCAFDoc_AssemblyItemRef' and m in {'Get','GetItem','IsOrphan','IsGUID','IsSubshapeIndex','GetGUID','GetSubshapeIndex'}:target='xde-attribute-projections'
 if f=='XCAFDoc_AssemblyItemId' and m=='ToString':target='xde-attribute-projections'
 if target is None:continue
 st=states[d['StableId']]['State'];assert st in {'Emitted','Manual','Blocked'},(s,st)
 op=dict(stableId=d['StableId'],nativeName=d['NativeName'],signature=s,guard='ASDocumentBound',reason='Explicitly used private prerequisite; already-bound IDs remain reused, not new migration.')
 support[target].append(op);(prerequisites if st=='Blocked' else reused).append(dict(op,workflow=target,baselineDisposition=st))
for resource,ops in operations.items():write(w/f'src/OcctSharp.Generator/Transformation/{resource}-operation-contracts.json',dict(schemaVersion='1.0',owners=[],operations=ops,supportingOperations=support[resource]))
source=Path('C:/Users/yoiri/source/repos/OCCT');commit=read(r/'sdk/source-identity.json')['commit'];identity=read(r/'sdk/source-identity.json')
for f in ('TDF_Transaction.lxx','TDF_Tool.lxx','TDF_Label.hxx','TDF_Label.cxx','NCollection_Array1.hxx','NCollection_HArray1.hxx'):
 matches=[p for p in subprocess.check_output(['git','ls-tree','-r','--name-only',commit],cwd=source,text=True).splitlines() if Path(p).name==f]
 for p in matches:
  data=subprocess.check_output(['git','show',commit+':'+p],cwd=source);(r/'sdk'/f).write_bytes(data)
  if p not in {x['path'] for x in identity['files']}:identity['files'].append(dict(path=p,sha256=hashlib.sha256(data).hexdigest().upper()))
write(r/'sdk/source-identity.json',identity)
write(docs/'BATCH_AS_DOCUMENT_ATTRIBUTES_CANDIDATES.json',dict(schemaVersion='1.0',state='AUDIT_INPUT_NOT_ADMISSION',baseline='8.0.1-preview.42',inventorySha256=sha(r/'preview42-baseline/inventory.json'),families=dict(Counter(x['NativeName'].split('::')[0] for x in primary)),candidates=primary))
write(docs/'BATCH_AS_DOCUMENT_ATTRIBUTES_ADMISSION.json',dict(schemaVersion='1.0',state='AUDITED_IMPLEMENTING',baseline='8.0.1-preview.42',inventorySha256=sha(r/'preview42-baseline/inventory.json'),sdkSourceCommit=commit,sdkSourceIdentitySha256=sha(r/'sdk/source-identity.json'),declarations=audit,prerequisites=prerequisites,reused=reused))
print(dict(candidates=len(primary),admitted=sum(map(len,operations.values())),deferred=sum(x['decision']=='DEFERRED' for x in audit),newPrerequisites=len(prerequisites),reused=len(reused),delta=sum(map(len,operations.values()))+len(prerequisites),byRule={k:len(v) for k,v in operations.items()}))

# Persistence support is private application-local C++, never a public callback
# rule. Track Blocked -> Manual independently of the generated admission delta.
manual=[]
for d in found.values():
 f,_,m=d['NativeName'].partition('::');s=d['NativeSignature']
 selected=(f in {'BinMDataStd_NamedDataDriver','XmlMDataStd_NamedDataDriver'} and m=='Paste') or (f=='TDocStd_Application' and m=='DefineFormat') or (f in {'XmlDrivers_DocumentStorageDriver','XmlXCAFDrivers_DocumentStorageDriver'} and s==f'{f}::{f}: void (const TCollection_ExtendedString &)') or s in {'XCAFDoc_Note::Set: void (const TCollection_ExtendedString &, const TCollection_ExtendedString &)', 'XCAFDoc_NoteComment::Set: void (const TCollection_ExtendedString &)'}
 if selected:
  assert states[d['StableId']]['State']=='Blocked' and d['Access']=='Public',d
  manual.append(dict(stableId=d['StableId'],nativeName=d['NativeName'],signature=s,header=d['Header'],baselineDisposition='Blocked',decision='MANUAL_PRIVATE_SUPPORT',specialCaseId='SC-062',reason='Application-local persistence adapter with bounded copied extension; no mutable attribute/driver/callback exported.'))
assert len(manual)==9
config=read(w/'config/generation.json');existing={x['stableId'] for x in config['manualBindings']}
config['manualBindings'] += [dict(stableId=x['stableId'],specialCaseId='SC-062') for x in manual if x['stableId'] not in existing];write(w/'config/generation.json',config)
identity=read(r/'sdk/source-identity.json')
sdk_files={'TDocStd_Application.cxx','NCollection_IndexedDataMap.hxx','BinMDataStd_NamedDataDriver.cxx','XmlMDataStd_NamedDataDriver.cxx','XmlMXCAFDoc_NoteCommentDriver.cxx','XmlMXCAFDoc_NoteDriver.cxx','XmlMDF_DerivedDriver.hxx','BinMDF_ADriverTable.cxx','XmlMDF_ADriverTable.cxx'}
for path in subprocess.check_output(['git','ls-tree','-r','--name-only',commit],cwd=source,text=True).splitlines():
 if Path(path).name in sdk_files:
  data=subprocess.check_output(['git','show',commit+':'+path],cwd=source);(r/'sdk'/Path(path).name).write_bytes(data)
  if path not in {x['path'] for x in identity['files']}:identity['files'].append(dict(path=path,sha256=hashlib.sha256(data).hexdigest().upper()))
write(r/'sdk/source-identity.json',identity)
admission=read(docs/'BATCH_AS_DOCUMENT_ATTRIBUTES_ADMISSION.json');admission['manualSupportingOperations']=manual;admission['sdkSourceIdentitySha256']=sha(r/'sdk/source-identity.json')
drivers={'BinDrivers_DocumentRetrievalDriver','BinDrivers_DocumentStorageDriver','BinXCAFDrivers_DocumentRetrievalDriver','BinXCAFDrivers_DocumentStorageDriver','XmlDrivers_DocumentRetrievalDriver','XmlDrivers_DocumentStorageDriver','XmlXCAFDrivers_DocumentRetrievalDriver','XmlXCAFDrivers_DocumentStorageDriver'}
persistence_reused=[]
for d in found.values():
 f,_,m=d['NativeName'].partition('::');s=d['NativeSignature']
 used=(f in drivers and m=='AttributeDrivers') or (f in drivers and s==f'{f}::{f}: void ()') or (f in {'BinMDataStd_NamedDataDriver','XmlMDataStd_NamedDataDriver','XmlMXCAFDoc_NoteCommentDriver'} and s==f'{f}::{f}: void (const occ::handle<Message_Messenger> &)') or (f in {'BinMDF_ADriverTable','XmlMDF_ADriverTable'} and m=='AddDriver') or (f=='TDataStd_NamedData' and m in {'Clear','Restore'})
 if used:
  assert states[d['StableId']]['State']=='Emitted'
  persistence_reused.append(dict(stableId=d['StableId'],nativeName=d['NativeName'],signature=s,baselineDisposition='Emitted'))
assert len(persistence_reused)==21 and not {x['stableId'] for x in persistence_reused}&{x['stableId'] for x in reused}
admission['persistenceReusedOperations']=persistence_reused
manual_ids={x['stableId'] for x in manual}
for item in admission['declarations']:
 if item['stableId'] in manual_ids:item.update(decision='MANUAL_PRIVATE_SUPPORT',reason='Private SC-062 XML note retrieval mutator; not a generated or public mutable-attribute contract.')
write(docs/'BATCH_AS_DOCUMENT_ATTRIBUTES_ADMISSION.json',admission)
print(dict(generatedDelta=62,manualDelta=9,primaryGenerated=49,primaryManual=2,primaryDeferred=112))
