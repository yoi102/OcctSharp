"""Freeze exact private exchange-session admission against delivered Preview.43."""
import hashlib,json
from pathlib import Path
from collections import Counter
w=Path(__file__).resolve().parents[1];r=w/'artifacts/batch-at';docs=w.parent/'docs'
read=lambda p:json.loads(p.read_text(encoding='utf-8-sig'))
write=lambda p,x:p.write_text(json.dumps(x,indent=2)+'\n',encoding='utf-8')
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest().upper()
rows=read(r/'family-audit-input.json');primary=[d for d in rows if d['State']=='Blocked'];assert len(primary)==182
found={d['StableId']:d for d in read(w/'artifacts/generator-determinism/gp-pnt-discovery-1.json')['Model']['Declarations']}
states={d['StableId']:d for d in read(r/'preview43-baseline/inventory.json')['FinalClassification']['Declarations']}
header={'HasFn','HasFd','HasFs','IsDone','FnValue','FdValue','Name','TimeStamp','Author','Organization','PreprocessorVersion','OriginatingSystem','Authorisation','Description','ImplementationLevel','SchemaIdentifiers','SetName','SetTimeStamp','SetAuthor','SetOrganization','SetPreprocessorVersion','SetOriginatingSystem','SetAuthorisation','SetDescription'}
iges={'SendName','ReceiveName','FileName','SystemId','InterfaceVersion','Date','AuthorName','CompanyName','LastChangeDate','ApplicationProtocol','UnitName','HasLastChangeDate','HasApplicationProtocol','CopyRefs','SetSendName','SetReceiveName','SetFileName','SetSystemId','SetInterfaceVersion','SetDate','SetAuthorName','SetCompanyName','SetLastChangeDate'}
ws={'LoadedFile','CategoryName','ValidityName','EvalSelection','SelectionResult','Shareds','Sharings','NumberFromLabel','ModelCheckList','CheckOne','LastRunCheckList','WriteFile'}
def choose(d):
 f,_,m=d['NativeName'].partition('::');s=d['NativeSignature']
 if f=='APIHeaderSection_MakeHeader':return m in header or s=='APIHeaderSection_MakeHeader::APIHeaderSection_MakeHeader: void (const occ::handle<StepData_StepModel> &)'
 if f=='HeaderSection_FileName':return m=='Init'
 if f=='HeaderSection_FileDescription':return m=='Init'
 if f=='IGESData_GlobalSection':return m in iges and s!='IGESData_GlobalSection::SetLastChangeDate: void ()'
 if f=='IFSelect_WorkSession':return m in ws and s!='IFSelect_WorkSession::WriteFile: IFSelect_ReturnStatus (const char *const, const occ::handle<IFSelect_Selection> &)'
 if f=='IFSelect_SelectType':return m=='ExtractLabel'
 return False
ops=[];audit=[]
for d in primary:
 admit=choose(d);x=found[d['StableId']];assert not admit or x['Access']=='Public'
 reason='Private initialized reader model; same-session IDs; bounded detached text/checks; temporary copied header replacement restored before returning.' if admit else 'Mutable external models/actors, streams, registry mutations, RTTI, redundant alias getters, arbitrary schema/unit changes or unproved receivers are outside the closed file workflow.'
 if 'GiveListCombined' in d['NativeName']:reason='Pinned SDK declares a null output sequence and calls list->Append on nonempty results; unsafe implementation deferred, never called.'
 item=dict(stableId=d['StableId'],nativeName=d['NativeName'],signature=d['NativeSignature'],guard='ATPrivateSession',reason=reason)
 audit.append(dict(item,header=d['Header'],baselineDisposition=d['State'],decision='ADMITTED' if admit else 'DEFERRED'))
 if admit:ops.append(item)
support=[];prerequisites=[];reused=[]
support_methods={'IFSelect_WorkSession':{'SetErrorHandle','ComputeGraph','EntityLabel','EntityName','ClearFinalModifiers','ClearShareOut'},'XSControl_Reader':{'WS','Model','ReadFile'},'IFSelect_SelectDeduct':{'SetInput'},'IFSelect_SelectPointed':{'SetList'},'Interface_InterfaceModel':{'NbEntities','Value','Number','NextNumberForLabel'},'StepData_StepModel':{'Header','ClearHeader','AddHeaderEntity'},'IGESData_IGESModel':{'GlobalSection','SetGlobalSection'},'IGESData_GlobalSection':{'UnitFlag','Scale','CascadeUnit','Resolution'},'IFSelect_SelectType':{'TypeForMatch'}}
constructors={'STEPControl_Reader::STEPControl_Reader: void ()','IGESControl_Reader::IGESControl_Reader: void ()','IFSelect_SelectModelEntities::IFSelect_SelectModelEntities: void ()','IFSelect_SelectModelRoots::IFSelect_SelectModelRoots: void ()','IFSelect_SelectPointed::IFSelect_SelectPointed: void ()','IFSelect_SelectType::IFSelect_SelectType: void (const occ::handle<Standard_Type> &)','HeaderSection_FileName::HeaderSection_FileName: void ()','HeaderSection_FileDescription::HeaderSection_FileDescription: void ()','StepData_StepModel::StepData_StepModel: void ()'}
for x in found.values():
 f,_,m=x['NativeName'].partition('::');s=x['NativeSignature']
 if not (m in support_methods.get(f,set()) or s in constructors):continue
 st=states[x['StableId']]['State'];assert st in {'Emitted','Manual','Blocked'},(s,st)
 assert x['Access']=='Public'
 item=dict(stableId=x['StableId'],nativeName=x['NativeName'],signature=s,guard='ATPrivateSession',reason='Audited private workflow prerequisite; existing emitted/manual declarations remain reuse.')
 support.append(item);(prerequisites if st=='Blocked' else reused).append(dict(item,baselineDisposition=st))
write(w/'src/OcctSharp.Generator/Transformation/exchange-sessions-operation-contracts.json',dict(schemaVersion='1.0',owners=[],operations=ops,supportingOperations=support))
write(docs/'BATCH_AT_EXCHANGE_SESSIONS_CANDIDATES.json',dict(schemaVersion='1.0',state='AUDIT_INPUT_NOT_ADMISSION',baseline='8.0.1-preview.43',inventorySha256=sha(r/'preview43-baseline/inventory.json'),families=dict(Counter(d['NativeName'].split('::')[0] for d in primary)),candidates=primary))
write(docs/'BATCH_AT_EXCHANGE_SESSIONS_ADMISSION.json',dict(schemaVersion='1.0',state='AUDITED_IMPLEMENTING',baseline='8.0.1-preview.43',inventorySha256=sha(r/'preview43-baseline/inventory.json'),sdkSourceCommit=read(r/'sdk/source-identity.json')['commit'],sdkSourceIdentitySha256=sha(r/'sdk/source-identity.json'),declarations=audit,prerequisites=prerequisites,reused=reused))
print(dict(candidates=len(primary),admitted=len(ops),deferred=len(primary)-len(ops),newPrerequisites=len(prerequisites),reused=len(reused),delta=len(ops)+len(prerequisites)))
