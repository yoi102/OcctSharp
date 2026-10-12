"""Freeze AV against the delivered AU baseline and exact copied-value scope."""
from pathlib import Path
import json,hashlib,shutil
w=Path(__file__).resolve().parents[1];docs=w.parent/'docs';r=w/'artifacts/batch-av'
read=lambda p:json.loads(p.read_text(encoding='utf-8-sig'))
write=lambda p,v:p.write_text(json.dumps(v,indent=2)+'\n',encoding='utf-8')
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest().upper()
assert read(w/'artifacts/batch-au/delivery-result.json')['state']=='PASS'
b=r/'preview45-baseline';b.mkdir(exist_ok=True)
if not (b/'manifest.json').exists():
    for source,name in [('generated/manifest.json','manifest.json'),('artifacts/generator-reports/full-inventory.json','inventory.json'),('artifacts/batch-au/api-current.json','api.json'),('artifacts/native/Release/OcctSharp.Native.dll','OcctSharp.Native.dll'),('artifacts/batch-au/delivery-result.json','delivery-result.json')]:shutil.copy2(w/source,b/name)
    for f in read(b/'manifest.json')['files']:
        dest=b/'generated'/f['relativePath'];dest.parent.mkdir(parents=True,exist_ok=True);shutil.copy2(w/f['relativePath'],dest)
primary=read(r/'preliminary-audit-input.json')['declarations']
selected=[];deferred=[]
asciiNames={'Capitalize','Clear','Length','UsefullLength','IsEmpty','IsAscii','LeftAdjust','RightAdjust','LowerCase','UpperCase','IsIntegerValue','IsRealValue','IntegerValue','RealValue','Center','LeftJustify','RightJustify','Remove','RemoveAll','Trunc','SubString','Split','Value','ChangeAll','Token','ToCString'}
commonNames={'Cat','AssignCat','Copy','Insert','Prepend','Search','SearchFromEnd','StartsWith','EndsWith','IsEqual','IsDifferent','IsGreater','IsLess'}
pathNames={'AbsolutePath','RelativePath','Disk','DownTrek','Extension','Name','Node','Password','Trek','UserName','IsAbsolutePath','IsContentProtocolPath','IsDosPath','IsNtExtendedPath','IsRelativePath','IsRemoteProtocolPath','IsUncExtendedPath','IsUncPath','IsUnixPath','IsValid','SystemName','TrekLength','TrekValue','UpTrek','SetName','SetExtension','SetTrek','Values'}
def choose(d):
    f,_,m=d['NativeName'].partition('::');s=d['NativeSignature']
    # The pinned header declares this overload but the installed baseline TKernel
    # import library does not export it (LNK2019 during AV native link).
    if s=='TCollection_AsciiString::IsEqual: bool (const TCollection_AsciiString &, const char *const)':return False
    if f in {'TCollection_AsciiString','TCollection_ExtendedString'}:
        if m==f:return s.endswith('void () noexcept') or s.endswith('void (const '+f+' &)') or (s.endswith('void (const char *const)') if f.endswith('AsciiString') else s.endswith('void (const char *const, const bool)'))
        if m in commonNames or m in {'InsertAfter','InsertBefore'}:
            return 'const '+f+' &' in s and s.count(f+' &')==1
        if m=='IsSameString':return s.count('const '+f+' &')==(2 if f.endswith('AsciiString') else 1)
        if f.endswith('AsciiString'):return m in asciiNames
        return m in {'Length','LengthOfCString','ToUTF8CString','ToExtString','IsEmpty','IsAscii','SubString','Split','Remove','Trunc','Value','IsIntegerValue','IsRealValue','IntegerValue','RealValue'}
    if f=='OSD_Path':return m in pathNames or (m==f and (s.endswith('void (const TCollection_AsciiString &, const OSD_SysType)') or s.endswith('void (const OSD_Path &)')))
    if f=='Quantity_Color':return m not in {'DumpJson','InitFromJson','operator const NCollection_Vec3<float> &'} and '&&' not in s and 'approx22' not in m and not (m==f and 'NCollection_Vec3' in s)
    if f=='UnitsAPI':return m in {'AnyToSI','AnyFromSI','AnyToAny','Dimensions'} or m.startswith('Dimension')
    return False
for d in primary:
    if d['baselineClassification']['State']!='Blocked':continue
    admitted=choose(d) and d['Access']=='Public' and not d.get('IsTemplated') and not d.get('IsPureVirtual')
    item=dict(stableId=d['StableId'],nativeName=d['NativeName'],signature=d['NativeSignature'],header=d['Header'],baselineDisposition='Blocked',decision='ADMITTED' if admitted else 'DEFERRED',guard='AVCopiedValues',reason='Initialized call-private value; bounded validated inputs; immutable success-only copied output and independent owned color clone.' if admitted else 'Redundant overload, stream/global mutation, environment/filesystem operation, move, borrowed storage or unproved initialization is outside this value profile.')
    if d['NativeSignature']=='TCollection_AsciiString::IsEqual: bool (const TCollection_AsciiString &, const char *const)':
        item['reason']='Pinned public header declares the overload but installed baseline TKernel import library lacks it; AV native link produced LNK2019. Not emulated.'
    (selected if admitted else deferred).append(item)
found=read(w/'artifacts/generator-determinism/gp-pnt-discovery-1.json')['Model']['Declarations']
states={d['StableId']:d['State'] for d in read(b/'inventory.json')['FinalClassification']['Declarations']}
supportNames={'Units_UnitSentence':{'Units_UnitSentence'},'Units_Sentence':{'IsDone','Evaluate'},'Units_Token':{'Dimensions'},'Units_Dimensions':{'IsEqual','Mass','Length','Time','ElectricCurrent','ThermodynamicTemperature','AmountOfSubstance','LuminousIntensity','PlaneAngle','SolidAngle'},'Quantity_Color':{'Quantity_Color','Name','Red','Green','Blue','Values','Convert_sRGB_To_LinearRGB','Convert_LinearRGB_To_sRGB'}}
support=[];new=[];reused=[]
primaryIds={x['stableId'] for x in selected}
for d in found:
    f,_,m=d['NativeName'].partition('::');sig=d['NativeSignature']
    if m not in supportNames.get(f,set()) or d['Access']!='Public' or d['StableId'] in primaryIds or states[d['StableId']]=='Skipped':continue
    if f=='Units_UnitSentence' and sig!='Units_UnitSentence::Units_UnitSentence: void (const char *const)':continue
    if f=='Quantity_Color' and m=='Quantity_Color' and sig!='Quantity_Color::Quantity_Color: void ()':continue
    if f=='Quantity_Color' and m.startswith('Convert_') and 'NCollection_Vec3<float>' not in sig:continue
    if f=='Units_Token' and sig!='Units_Token::Dimensions: occ::handle<Units_Dimensions> () const':continue
    item=dict(stableId=d['StableId'],nativeName=d['NativeName'],signature=sig,guard='AVCopiedValues',reason='Private unit validation or existing scalar color construction; no borrowed value escapes.')
    support.append(item);(new if states[d['StableId']]=='Blocked' else reused).append(dict(item,baselineDisposition=states[d['StableId']]))
write(w/'src/OcctSharp.Generator/Transformation/foundation-value-workspaces-operation-contracts.json',dict(schemaVersion='1.0',owners=[],operations=[{k:x[k] for k in ['stableId','nativeName','signature','guard','reason']} for x in selected],supportingOperations=support))
write(docs/'BATCH_AV_FOUNDATION_VALUES_CANDIDATES.json',dict(schemaVersion='1.0',state='AUDIT_INPUT_NOT_ADMISSION',baseline='8.0.1-preview.45',inventorySha256=sha(b/'inventory.json'),candidates=[d for d in primary if d['baselineClassification']['State']=='Blocked']))
write(docs/'BATCH_AV_FOUNDATION_VALUES_ADMISSION.json',dict(schemaVersion='1.0',state='AUDITED_IMPLEMENTING',baseline='8.0.1-preview.45',inventorySha256=sha(b/'inventory.json'),sdkSourceCommit=read(r/'sdk/source-identity.json')['commit'],sdkSourceIdentitySha256=sha(r/'sdk/source-identity.json'),declarations=selected+deferred,prerequisites=new,reused=reused))
print(dict(primary=len(selected)+len(deferred),admitted=len(selected),deferred=len(deferred),newPrerequisites=len(new),reused=len(reused),generatedDelta=len(selected)+len(new)))
