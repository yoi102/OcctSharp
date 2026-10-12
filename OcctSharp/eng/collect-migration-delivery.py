"""Freeze a migration delivery only after independently asserted local evidence."""
import argparse, hashlib, json, re, subprocess
from datetime import datetime, timedelta, timezone
from pathlib import Path
from migration_delivery_delta import verify_inventory_delta
from migration_generated_outputs import verify_generated_output_hashes
from migration_delivery_evidence import verify_final_runtime, verify_visible_result, verify_configured_mesh_result, verify_round_position, verify_native_probe, verify_boolean_repair_result, verify_local_feature_result, verify_mesh_adapter_result, verify_document_adapter_result
from migration_fixture_frames import verify_fixture_frame
from migration_delivery_evidence import verify_located_assembly_result
from migration_step_fidelity import verify_step_profiles
from migration_owned_resources import verify_resource_reports

p=argparse.ArgumentParser()
for name in ['batch','version','abi','bridge','baseline']: p.add_argument('--'+name,required=True)
for name in ['generator','runtime','delta','exports','remaining']: p.add_argument('--'+name,type=int,required=True)
p.add_argument('--manual-delta',type=int,default=0)
a=p.parse_args();w=Path(__file__).resolve().parents[1];repo=w.parent;r=w/'artifacts'/('batch-'+a.batch)
verify_round_position(a.batch,a.remaining)
read=lambda p:json.loads(p.read_text(encoding='utf-8-sig'))
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest().upper()
log=(r/'release-check-initial.log').read_text(encoding='utf-8-sig')
assert 'Release engineering checks completed.' in log
for n in [a.generator,a.runtime]:
    assert len(re.findall(r'Passed:\s+'+str(n)+r', Skipped:\s+0',log))==3,(n,'Release/Debug/cold suites')
gates=read(w/'artifacts/release/release-gates.json')
assert gates['packageVersion']==a.version and gates['batchImplementationComplete'] and not gates['publicReleaseReady']
names=['source-audit.json','native-exports.json','package-content.json','package-payloads.json','documentation-qa.json',
       'integration-actual-debug/result.json','integration-debug-loaded-modules.json','private-headers/result.json']
if a.batch=='cf':names.append('wpf-frame-reference-review.json')
if a.batch in ('bt','bu','bv','bw','bx','by','bz','ca','cb','cc','cd','ce','cf','cg'):
    names.append('all-resource-input-verification.json')
    resources=read(r/'all-resource-input-verification.json')
    assert resources['state']=='PASS' and resources['count']==len(resources['files'])
    for entry in resources['files']:assert sha(w/entry['path'])==entry['sha256']
    verify_final_runtime((r/'runtime-final-package-payloads.log').read_text(encoding='utf-8-sig'),a.runtime)
for name in names: assert read(r/name)['state']=='PASS',name
final=(r/'package-final.log').read_text(encoding='utf-8-sig')
assert 'Package consumer passed with 62 DLLs' in final
assert 'Direct module package consumer verified 62 shared native DLLs without the facade package.' in final
debug=read(r/'integration-actual-debug/result.json');loaded=read(r/'integration-debug-loaded-modules.json')
assert debug['tests']==a.runtime and debug['debugBridgeSha256']==sha(w/'artifacts/native/Debug/OcctSharp.Native.dll')
assert sha(Path(debug['runDirectory'])/'OcctSharp.Runtime.Tests.dll')==sha(w/'tests/OcctSharp.Runtime.Tests/bin/Debug/net10.0/win-x64/OcctSharp.Runtime.Tests.dll')
assert loaded['nativeDlls']==62
assert loaded.get('runtimeState','PASS')=='PASS' and loaded.get('failed',0)==0, 'Loaded identities cannot replace passing runtime tests'
for entry in loaded['native']: assert sha(Path(entry['path']))==entry['sha256']==sha(w/'artifacts/native/Debug'/entry['name'])
exports=read(r/'native-exports.json');api=read(r/'api-current.json');diff=read(r/'api-diff.json')
assert exports['removed']==0 and len(exports['added'])==a.exports and exports['debugMatchesRelease']
assert diff['RemovedCount']==0 and not diff['HasBreakingChanges']
c11=[]
for config in ['release','debug']:
    assert '250 release races / 16000 acquisitions' in (r/f'owned-{config}.log').read_text(encoding='utf-8-sig')
    text=(r/f'geometric-{config}.log').read_text(encoding='utf-8-sig')
    found=re.search(r'Geometric ABI PASS: '+config.title()+r', (\d+) C11 headers, thirty-four native size/offset assertions',text)
    assert found,text[-200:];c11.append(int(found[1]))
assert c11[0]==c11[1]
full=read(w/'artifacts/generator-reports/full-inventory.json');base=read(r/a.baseline/'inventory.json');inventory=full['FinalClassification']
assert inventory['IsComplete'] and inventory['DeclarationPending']==0
audit=read(r/'source-audit.json')
if a.batch=='cg':
    from migration_viewer_recovery import verify_recovery
    checkpoint=read(r/'unified-checkpoint-result.json')
    assert checkpoint['state']=='PASS' and checkpoint['bothCurrentFullSuitesRetained'] and checkpoint['officialSuffixCompleted']
    assert checkpoint['combinedLogSha256']==sha(r/'release-check-initial.log') and checkpoint['originalPassedLogSha256']==sha(r/'release-check-1-bundle-abi.log')
    assert checkpoint['continuationLogSha256']==sha(r/'release-check-resume.log') and checkpoint['passedCompiledInputsSha256']==sha(r/'passed-compiled-inputs.json')
    fault_source=read(r/'fault-source-audit.json')
    assert fault_source['state']=='PASS' and fault_source['fixedSdkSourceFiles']==54 and fault_source['physicalGpuRemoval']=='NOT RUN'
    assert sha(w/fault_source['testOnlyShimPath'])==fault_source['shimSourceSha256']
    callers=read(r/'manual-caller-equivalence.json')
    assert callers['state']=='PASS' and callers['manualToEmitted']==80 and callers['semanticCalls']==114 and callers['configurationExact']
    assert len(callers['callers'])==7 and callers['admittedSupportWorkflow']=='render-health-v1'
    for row in callers['callers']:assert sha(w/row['path'])==row['sha256']
    review=read(r/'recovery-frame-reference-review.json')
    assert review['state']=='PASS' and review['reference']=='recovery-release-4' and review['directlyInspectedDistinctFrames']==3
    for row in review['files']:assert sha(r/row['path'])==row['sha256']
    for context,configuration in [('recovery-release-4','Release'),('recovery-debug','Debug'),('recovery-package','Release')]:
        products=read(r/'package-payloads.json')['managedPayload'] if configuration=='Release' else [dict(name=row['name'],sha256=sha(w/'src'/Path(row['name']).stem/'bin/Debug/net10.0/win-x64'/row['name'])) for row in read(r/'package-payloads.json')['managedPayload']]
        verify_recovery(r/context,w/'artifacts/native'/configuration,r/'recovery-release-4',r/('shim-'+configuration)/'ContextFaultShim.dll',products)
    assert 'Ran 10 tests' in (r/'recovery-evidence-fixtures.log').read_text(encoding='utf-8-sig')
    private=(repo/'docs/STATUS.md').read_bytes();assert private[private.index(b'### Cadoryx'):]==(r/'status-private-baseline.txt').read_bytes()
    assert not any('ContextFaultShim' in name for name in __import__('zipfile').ZipFile(w/f'artifacts/packages/OcctSharp.Native.win-x64.{a.version}.nupkg').namelist())
if a.batch=='cf':
    from migration_wpf_input import verify_wpf_input
    image_review=read(r/'wpf-frame-reference-review.json')
    assert image_review['state']=='PASS' and image_review['reference']=='wpf-release-2' and len(image_review['files'])==9
    for row in image_review['files']:assert sha(r/row['path'])==row['sha256']
    callers=read(r/'manual-caller-equivalence.json')
    assert callers['state']=='PASS' and callers['manualToEmitted']==40 and callers['semanticCalls']==63 and callers['configurationExact']
    assert len(callers['callers'])==11 and callers['templateSemanticCalls']==4
    for row in callers['callers']:assert sha(w/row['path'])==row['sha256']
    for context,configuration in [('wpf-release-2','Release'),('wpf-debug-2','Debug'),('wpf-package','Release')]:
        verify_wpf_input(r/context,w/'artifacts/native'/configuration,r/'wpf-release-2')
    original=read(r/'wpf-repro/capture-loss-reproduction.json')
    assert original['state']=='FAIL' and original['staleClick'] is True
    assert 'Ran 9 tests' in (r/'wpf-evidence-fixtures.log').read_text(encoding='utf-8-sig')
    private=(repo/'docs/STATUS.md').read_bytes();private=private[private.index(b'### Cadoryx'):]
    assert private==(r/'status-private-baseline.txt').read_bytes()
if a.batch=='ce':
    callers=read(r/'manual-caller-equivalence.json')
    assert callers['state']=='PASS' and callers['manualToEmitted']==34 and callers['configurationExact'] and callers['semanticCalls']==38
    assert len(callers['callers'])==2 and len(callers['unchangedRelevantCallers'])==3 and len(callers['deferred'])==6
    assert callers['policyCorrections']==[dict(path='src/OcctSharp.Native/src/Exchange/XdeExchange.cpp',field='source_length_unit_meters',before='SDK UnitValue ratio',after='model ratio * model CascadeUnit * .001',sdkReason='fixed UnitFlagValue returns millimeters; CascadeUnit is captured in model')]
    for row in callers['callers']+callers['unchangedRelevantCallers']:assert sha(w/row['path'])==row['sha256']
    for context in ('actual-debug-resources','final-resources','facade-resources'):
        verify_resource_reports(r/context,w/'tests/Fixtures/OwnedMeshResources')
    verify_resource_reports(r/'module-resources',w/'tests/Fixtures/OwnedMeshResources',module=True)
    assert 'Owned resource evidence fixtures PASS: 1 positive/47 negative cases' in (r/'resource-evidence-fixtures.log').read_text(encoding='utf-8-sig')
    private=(repo/'docs/STATUS.md').read_bytes();private=private[private.index(b'### Cadoryx'):]
    assert private==(r/'status-private-baseline.txt').read_bytes()
if a.batch=='cd':
    checkpoint=read(r/'unified-checkpoint-result.json')
    assert checkpoint['state']=='PASS' and checkpoint['bothCurrentFullSuitesRetained'] and checkpoint['officialSuffixCompleted']
    assert checkpoint['combinedLogSha256']==sha(r/'release-check-initial.log') and checkpoint['originalPassedLogSha256']==sha(r/'release-check-1-duplicate-docs.log')
    assert checkpoint['continuationLogSha256']==sha(r/'release-check-resume.log') and checkpoint['passedCompiledInputsSha256']==sha(r/'passed-compiled-inputs.json')
    assert 'OK' in (r/'checkpoint-tooling-fixtures-2.log').read_text(encoding='utf-8-sig')
    callers=read(r/'manual-caller-equivalence.json')
    assert callers['state']=='PASS' and callers['manualToEmitted']==11 and callers['configurationExact'] and callers['semanticCalls']==23
    assert len(callers['callers'])==2 and not callers['unchangedRelevantCallers'] and len(callers['deferred'])==1
    for row in callers['callers']:assert sha(w/row['path'])==row['sha256']
    ledger=read(w/'tests/Fixtures/ConfiguredStep/fidelity.json')
    assert ledger['fixedSdkCommit']=='b8f597c677811d1f9f4d8a97f5ae2825c0353a42'
    assert set(ledger['profiles'])=={'Ap203-Facade','Ap214-Facade','Ap242-Facade','Ap242-NativeRewrite','Ap242-Native'}
    for context in ('actual-debug-fields','final-fields','facade-fields'):
        verify_step_profiles(r/context,ledger,ledger['profiles'])
    verify_step_profiles(r/'module-fields',ledger,['Ap242-Native'])
    assert 'STEP fidelity tooling fixtures PASS: 49 positive/negative cases' in (r/'fidelity-fixtures.log').read_text(encoding='utf-8-sig')
    private=(repo/'docs/STATUS.md').read_bytes();private=private[private.index(b'### Cadoryx'):]
    assert private==(r/'status-private-baseline.txt').read_bytes()
if a.batch=='cc':
    checkpoint=read(r/'unified-checkpoint-result.json')
    assert checkpoint['state']=='PASS' and checkpoint['bothCurrentFullSuitesRetained'] and checkpoint['officialSuffixCompleted']
    assert checkpoint['originalPassedLogSha256']==sha(r/'release-check-6-unindexed-generated.log')
    assert checkpoint['continuationLogSha256']==sha(r/'release-check-resume.log') and checkpoint['combinedLogSha256']==sha(r/'release-check-initial.log')
    checkpoint_inputs=read(r/'unified-checkpoint-inputs.json')
    assert checkpoint_inputs['unifiedEntrySha256']==sha(w/'eng/release-check.ps1')
    for row in checkpoint_inputs['inputs']:
        path=Path(row['path'])
        if path.resolve()==(w/'src/OcctSharp/OcctSharp.csproj').resolve() and sha(path)!=row['sha256']:
            old_project=(r/'checkpoint-facade.csproj').read_text(encoding='utf-8-sig')
            assert sha(r/'checkpoint-facade.csproj')==row['sha256']
            anchor='    <None Include="..\\..\\..\\docs\\NEXT_ROUND_BF_BK_CANDIDATES.json"'
            for name in ('PROGRESS_PREVIEW_78_COUNTS.json','BATCH_CC_ASSEMBLY_ADMISSION.json'):
                entry=f'    <None Include="..\\..\\..\\docs\\{name}" Pack="true" PackagePath="docs\\" />\n'
                if entry not in old_project:old_project=old_project.replace(anchor,entry+anchor)
            assert path.read_text(encoding='utf-8-sig')==old_project,'Only final counts/admission packaging may change'
        else:assert sha(path)==row['sha256']
    old_sample=(r/'SampleProgram-before-bridge-fix.cs').read_text(encoding='utf-8-sig')
    assert (w/'samples/OcctSharp.Samples/Program.cs').read_text(encoding='utf-8-sig')==old_sample.replace('runtime.BridgeVersion != "0.126.0"','runtime.BridgeVersion != "0.127.0"')
    for configuration in ('release','debug'):
        assert 'OcctSharp smoke passed: ABI 1.113, bridge 0.127.0, OCCT 8.0.1, 62 DLLs, box faces 6, mesh triangles 12.' in (r/f'sample-{configuration}-smoke.log').read_text(encoding='utf-8-sig')
    restored=read(r/'desktop-restored-result.json')
    assert restored['state']=='PASS' and restored['runtimeTests']==7 and restored['failed']==0 and restored['skipped']==0
    assert restored['helperRestoredByteExact'] and restored['helperSha256']==sha(w/'tests/Shared/BatchAUDesktopCapture.cs')==sha(r/'DesktopCapture-before-timeout.cs')
    assert restored['logSha256']==sha(r/'desktop-restored-seven.log')
    assert restored['historicalFullFailureSha256']==sha(r/'release-check-4-desktop-timeout.log')
    assert restored['historicalFailedRetrySha256']==sha(r/'desktop-failed-seven-retry.log')
    verify_final_runtime((r/'desktop-restored-seven.log').read_text(encoding='utf-8-sig'),7)
    callers=read(r/'manual-caller-equivalence.json')
    assert callers['state']=='PASS' and callers['manualToEmitted']==137 and callers['newSdkDeclarations']==0 and callers['configurationExact']
    assert len(callers['callers'])==15 and len(callers['unchangedRelevantCallers'])==8 and len(callers['deferred'])==1
    for row in callers['callers']+callers['unchangedRelevantCallers']:assert sha(w/row['path'])==row['sha256']
    for configuration in ('release','debug'):
        verify_native_probe((r/f'adapter-{configuration}.log').read_text(encoding='utf-8-sig'),'XDE exact adapters/defaults/borrowed identity/SDK base copy and 5 noncommuting shared located leaves: PASS')
        verify_native_probe((r/f'adapter-{configuration}.log').read_text(encoding='utf-8-sig'),'Receiver-before-argument and temporary borrowed lifetime PASS;')
        verify_native_probe((r/f'adapter-{configuration}.log').read_text(encoding='utf-8-sig'),'Shared graph complete path depth/budget/domain rejection PASS;')
    proof=read(r/'auxiliary-inputs.json');assert proof['state']=='PASS' and len(proof['inputs'])>=400
    for row in proof['inputs']:assert sha(Path(row['path']))==row['sha256']
    review=read(r/'reference-frame-review.json');assert review['state']=='PASS' and len(review['images'])==2
    for row in review['images']:assert sha(r/row['path'])==row['sha256']
    private=(repo/'docs/STATUS.md').read_text(encoding='utf-8-sig');private=private[private.index('### Cadoryx'):]
    assert private==(r/'status-private-baseline.txt').read_text(encoding='utf-8')
    for context in ('actual-debug-visible','final-visible','facade-visible','module-visible'):
        current=r/context;reference=r/'inspected-reference'
        verify_located_assembly_result(read(current/'workflow-result.json'),read(current/'located/visible-result.json'),read(current/'oracle.json'),read(current/'rejection-result.json'))
        for name in ('assembly.step','part-1.stp','part-2.stp','located/edited.step'):assert (current/name).is_file()
        for name in ('before.png','selected.png'):verify_fixture_frame((current/'located'/name).read_bytes(),(reference/'located'/name).read_bytes())
if a.batch=='cb':
    checkpoint=read(r/'unified-checkpoint-result.json')
    assert checkpoint['state']=='PASS' and checkpoint['retainedBothBuildRuntimeSuites'] and checkpoint['currentFixtureColdReleasePassed']
    assert checkpoint['currentFixtureActualDebugRequired'] and checkpoint['originalPassedLogSha256']==sha(r/'release-check-1-module-fixture.log')
    assert checkpoint['continuationLogSha256']==sha(r/'release-check-resume.log') and checkpoint['combinedLogSha256']==sha(r/'release-check-initial.log')
    inputs=read(r/'unified-checkpoint-inputs.json');assert inputs['unifiedEntrySha256']==sha(w/'eng/release-check.ps1')
    for row in inputs['inputs']:
        path=Path(row['path'])
        if path.resolve()==(w/'src/OcctSharp/OcctSharp.csproj').resolve() and sha(path)!=row['sha256']:
            baseline=r/'checkpoint-facade.csproj';assert sha(baseline)==row['sha256']
            old=baseline.read_text(encoding='utf-8-sig');current=path.read_text(encoding='utf-8-sig')
            entry='    <None Include="..\\..\\..\\docs\\PROGRESS_PREVIEW_77_COUNTS.json" Pack="true" PackagePath="docs\\" />\n'
            anchor='    <None Include="..\\..\\..\\docs\\NEXT_ROUND_BF_BK_CANDIDATES.json"'
            assert entry not in old and current==old.replace(anchor,entry+anchor),'Only final counts packaging may change'
        else:assert sha(path)==row['sha256']
    callers=read(r/'manual-caller-equivalence.json')
    assert callers['state']=='PASS' and callers['manualToEmitted']==125 and callers['newSdkDeclarations']==0
    assert callers['configurationExact'] and callers['semanticCalls']==313 and callers['typedLookups']==45
    assert len(callers['callers'])==17 and len(callers['unchangedRelevantCallers'])==12 and len(callers['deferred'])==1
    for row in callers['callers']+callers['unchangedRelevantCallers']:assert sha(w/row['path'])==row['sha256']
    for configuration in ('release','debug'):
        verify_native_probe((r/f'adapter-{configuration}.log').read_text(encoding='utf-8-sig'),'Document adapters equivalence PASS:125 exact SDK signatures')
    proof=read(r/'auxiliary-inputs.json');assert proof['state']=='PASS' and len(proof['inputs'])>=400
    for row in proof['inputs']:assert sha(Path(row['path']))==row['sha256']
    review=read(r/'reference-frame-review.json');assert review['state']=='PASS' and len(review['images'])==4
    for row in review['images']:assert sha(r/row['path'])==row['sha256']
    private=(repo/'docs/STATUS.md').read_text(encoding='utf-8-sig');private=private[private.index('### Cadoryx'):]
    assert private==(r/'status-private-baseline.txt').read_text(encoding='utf-8')
    for context in ('actual-debug-visible','final-visible','facade-visible','module-visible'):
        for format in ('Binary','Xml'):
            current=r/context/format.lower();reference=r/'inspected-reference'/format.lower()
            verify_document_adapter_result(read(current/'workflow-result.json'),read(current/'visible-result.json'),format)
            assert (current/('document.xml' if format=='Xml' else 'document.xbf')).is_file() and (current/'edited.step').is_file()
            for name in ('before.png','selected.png'):verify_fixture_frame((current/name).read_bytes(),(reference/name).read_bytes())
if a.batch=='ca':
    checkpoint=read(r/'unified-checkpoint-result.json')
    assert checkpoint['state']=='PASS' and checkpoint['retainedBothBuildRuntimeSuites']
    assert checkpoint['originalPassedLogSha256']==sha(r/'release-check-2-bundle-version.log')
    assert checkpoint['continuationLogSha256']==sha(r/'release-check-resume.log')
    assert checkpoint['combinedLogSha256']==sha(r/'release-check-initial.log')
    inputs=read(r/'unified-checkpoint-inputs.json')
    assert inputs['unifiedEntrySha256']==sha(w/'eng/release-check.ps1')
    for row in inputs['inputs']:
        path=Path(row['path'])
        if path.resolve()==(w/'src/OcctSharp/OcctSharp.csproj').resolve() and sha(path)!=row['sha256']:
            baseline=r/'checkpoint-facade.csproj'
            assert sha(baseline)==row['sha256']
            old=baseline.read_text(encoding='utf-8-sig');current=path.read_text(encoding='utf-8-sig')
            entry='    <None Include="..\\..\\..\\docs\\PROGRESS_PREVIEW_76_COUNTS.json" Pack="true" PackagePath="docs\\" />\n'
            anchor='    <None Include="..\\..\\..\\docs\\NEXT_ROUND_BF_BK_CANDIDATES.json"'
            assert entry not in old and current==old.replace(anchor,entry+anchor),'Only the final counts document packaging entry may change'
        else:assert sha(path)==row['sha256']
    callers=read(r/'manual-caller-equivalence.json')
    assert callers['state']=='PASS' and callers['manualToEmitted']==24 and callers['newSdkDeclarations']==0
    assert callers['configurationExact'] and len(callers['callers'])==9 and len(callers['unchangedRelevantCallers'])==2
    for row in callers['callers']+callers['unchangedRelevantCallers']:assert sha(w/row['path'])==row['sha256']
    for configuration in ('release','debug'):
        verify_native_probe((r/f'adapter-{configuration}.log').read_text(encoding='utf-8-sig'),'Mesh adapters equivalence PASS:24 exact SDK signatures')
    proof=read(r/'auxiliary-inputs.json')
    assert proof['state']=='PASS' and len(proof['inputs'])>=400
    for row in proof['inputs']:assert sha(Path(row['path']))==row['sha256']
    review=read(r/'reference-frame-review.json')
    assert review['state']=='PASS' and len(review['images'])==4
    for row in review['images']:assert sha(r/row['path'])==row['sha256']
    private=(repo/'docs/STATUS.md').read_text(encoding='utf-8-sig')
    private=private[private.index('### Cadoryx'):]
    assert private==(r/'status-private-baseline.txt').read_text(encoding='utf-8')
    for context in ('actual-debug-visible','final-visible','facade-visible','module-visible'):
        for operation in ('Remesh','Patch'):
            current=r/context/operation.lower();reference=r/'inspected-reference'/operation.lower()
            verify_mesh_adapter_result(read(current/'workflow-result.json'),read(current/'visible-result.json'),operation)
            assert (current/('edited.brep' if operation=='Patch' else 'edited.step')).is_file() and (current/'source.xbf').is_file()
            for name in ('before.png','selected.png'):verify_fixture_frame((current/name).read_bytes(),(reference/name).read_bytes())
if a.batch=='bz':
    callers=read(r/'manual-caller-equivalence.json')
    assert callers['state']=='PASS' and callers['manualToEmitted']==111 and callers['newSdkDeclarations']==0
    assert callers['configurationExact'] and len(callers['callers'])==9
    for row in callers['callers']:assert sha(w/row['path'])==row['sha256']
    for configuration in ('release','debug'):
        verify_native_probe((r/f'adapter-{configuration}.log').read_text(encoding='utf-8-sig'),'Local-feature adapters equivalence PASS:ten builder families')
    proof=read(r/'auxiliary-inputs.json')
    assert proof['state']=='PASS' and len(proof['inputs'])>=400
    for row in proof['inputs']:assert sha(Path(row['path']))==row['sha256']
    review=read(r/'reference-frame-review.json')
    assert review['state']=='PASS' and len(review['images'])==6
    for row in review['images']:assert sha(r/row['path'])==row['sha256']
    for context in ('actual-debug-visible','final-visible','facade-visible','module-visible'):
        for operation in ('Fillet','Chamfer','Sweep'):
            current=r/context/operation.lower();reference=r/'inspected-reference'/operation.lower()
            verify_local_feature_result(read(current/'workflow-result.json'),read(current/'visible-result.json'),operation)
            assert (current/'edited.step').is_file()
            for name in ('before.png','selected.png'):verify_fixture_frame((current/name).read_bytes(),(reference/name).read_bytes())
if a.batch=='by':
    lane=read(r/'visible-lane-start.json')
    assert lane['state']=='PASS' and lane['visiblePeerWindows']==0
    checkpoint=read(r/'unified-checkpoint-result.json')
    assert checkpoint['state']=='PASS' and checkpoint['initialCommandState']=='FAIL' and checkpoint['reranBothRuntimeSuites']
    assert checkpoint['initialLogSha256']==sha(r/'release-check-1-private-fingerprint.log')
    assert checkpoint['continuationLogSha256']==sha(r/'release-check-resume.log')
    inputs=read(r/'unified-checkpoint-inputs.json')
    assert inputs['unifiedEntrySha256']==sha(w/'eng/release-check.ps1')
    for row in inputs['inputs']:assert sha(Path(row['path']))==row['sha256']
    callers=read(r/'manual-caller-equivalence.json')
    assert callers['state']=='PASS' and callers['manualToEmitted']==19 and callers['newSdkDeclarations']==0
    assert callers['configurationExact'] and len(callers['callers'])==2 and len(callers['unchangedRelevantCallers'])==7
    for row in callers['callers']+callers['unchangedRelevantCallers']:assert sha(w/row['path'])==row['sha256']
    for configuration in ('release','debug'):
        verify_native_probe((r/f'adapter-{configuration}.log').read_text(encoding='utf-8-sig'),'Boolean/repair adapters equivalence PASS:19 exact SDK calls')
    proof=read(r/'auxiliary-inputs.json')
    assert proof['state']=='PASS' and len(proof['inputs'])>=400
    for row in proof['inputs']:assert sha(Path(row['path']))==row['sha256']
    review=read(r/'reference-frame-review.json')
    assert review['state']=='PASS' and len(review['images'])==6
    for row in review['images']:assert sha(r/row['path'])==row['sha256']
    for context in ('actual-debug-visible','final-visible','facade-visible','module-visible'):
        for operation in ('Fuse','Cut','Common'):
            current=r/context/operation.lower();reference=r/'inspected-reference'/operation.lower()
            verify_boolean_repair_result(read(current/'workflow-result.json'),read(current/'visible-result.json'),operation)
            assert (current/'edited.step').is_file()
            for name in ('before.png','selected.png'):verify_fixture_frame((current/name).read_bytes(),(reference/name).read_bytes())
if a.batch=='bv':
    callers=read(r/'manual-caller-equivalence.json')
    assert callers['state']=='PASS' and callers['manualToEmitted']==17 and callers['newSdkDeclarations']==0
    for row in callers['callers']: assert sha(w/row['path'])==row['sha256']
    for configuration in ('release','debug'):
        assert 'Projection adapters equivalence PASS:17 exact SDK calls' in (r/f'adapter-{configuration}.log').read_text(encoding='utf-8-sig')
    boundary=read(r/'boundary-result.json')
    assert boundary['state']=='PASS' and boundary['boundaryCases']==8 and boundary['actualLoadedNativeDlls']==62
    for row in boundary['loaded']:
        assert sha(Path(row['path']))==row['sha256']==sha(w/'artifacts/native/Release'/Path(row['path']).name)
    proof=read(r/'auxiliary-inputs.json')
    assert proof['state']=='PASS' and len(proof['inputs'])>=400
    for row in proof['inputs']: assert sha(Path(row['path']))==row['sha256']
if a.batch=='bw':
    callers=read(r/'manual-caller-equivalence.json')
    assert callers['state']=='PASS' and callers['manualToEmitted']==18 and callers['newSdkDeclarations']==0
    assert len(callers['callers'])==2 and callers['configurationExact']
    for row in callers['callers']:assert sha(w/row['path'])==row['sha256']
    proof=read(r/'auxiliary-inputs.json')
    assert proof['state']=='PASS' and len(proof['inputs'])>=400
    for row in proof['inputs']:assert sha(Path(row['path']))==row['sha256']
    legacy=read(r/'legacy-baseline-result.json')
    assert legacy['state']=='PINNED_LEGACY_LIMIT_REPRODUCED' and legacy['twoSectionFailureReproduced'] and legacy['fourSectionMappingPassed']
    assert legacy['bridgeVersion']=='0.120.0' and legacy['abi']=='1.111'
    assert legacy['loadedBridgeSha256']=='B381FB9F4D368C9E8D6B2C981B729DC30A3F6A4ED1C1765858373C0EF6F70EF5'
    assert sha(Path(legacy['loadedBridgePath']))==legacy['loadedBridgeSha256']
    checkpoint=read(r/'unified-checkpoint-result.json')
    assert checkpoint['state']=='PASS' and checkpoint['initialCommandState']=='FAIL' and checkpoint['reranBothRuntimeSuites']
    for configuration in ('release','debug'):
        verify_native_probe((r/f'adapter-{configuration}.log').read_text(encoding='utf-8-sig'),'Fitting adapters equivalence PASS:18 exact SDK calls')
        verify_native_probe((r/f'filling-buffers-{configuration}.log').read_text(encoding='utf-8-sig'),'Nonuniform filling C ABI PASS:')
        verify_native_probe((r/f'two-section-sdk-{configuration}.log').read_text(encoding='utf-8-sig'),'Two-section SDK profiles PASS:4 spline geometries /8 exact StdFail_NotDone unsupported profiles.')
        verify_native_probe((r/f'appsweep-readonly-{configuration}.log').read_text(encoding='utf-8-sig'),'Pinned AppSweep section defect reproduced: expected final translation Z=3; actual=0; sections=21.')
if a.batch=='bx':
    recapture=read(r/'actual-debug-frame-recapture.json')
    assert recapture['state']=='PASS' and recapture['fullSuiteTests']==a.runtime and recapture['recapturedTests']==1
    assert recapture['runDirectory']==debug['runDirectory']
    assert recapture['fullTrxSha256']==sha(Path(debug['runDirectory'])/'actual-debug.trx')
    assert recapture['testAssemblySha256']==sha(Path(debug['runDirectory'])/'OcctSharp.Runtime.Tests.dll')
    assert recapture['sourceInputSnapshotSha256']==sha(r/'integration-debug-inputs.json')
    import xml.etree.ElementTree as ET
    focused=ET.parse(r/'actual-debug-frame-recapture/frame-recapture.trx').getroot().find('.//{*}Counters')
    assert focused is not None and all(int(focused.attrib[k])==v for k,v in {'total':1,'passed':1,'failed':0,'notExecuted':0}.items())
    assert len(recapture['native'])==62 and {x['name'] for x in recapture['native']}=={x['name'] for x in loaded['native']}
    for row in recapture['native']:
        assert Path(row['path']).parent==Path(debug['runDirectory'])/'occt'
        assert sha(Path(row['path']))==row['sha256']==sha(w/'artifacts/native/Debug'/row['name'])
    for row in recapture['images']:assert sha(r/'actual-debug-visible'/row['name'])==row['sha256']
    assert read(r/'visible-lane-start.json')['state']=='PASS'
    assert read(r/'visible-lane-start.json')['visiblePeerWindows']==0
    proof=read(r/'auxiliary-inputs.json')
    assert proof['state']=='PASS' and len(proof['inputs'])>=400
    for row in proof['inputs']:assert sha(Path(row['path']))==row['sha256']
    checkpoint=read(r/'unified-checkpoint-result.json')
    assert checkpoint['state']=='PASS' and checkpoint['initialCommandState']=='FAIL' and checkpoint['reranBothRuntimeSuites']
    assert checkpoint['initialLogSha256']==sha(r/'release-check-1-module-fixture.log')
    assert checkpoint['continuationLogSha256']==sha(r/'release-check-resume.log')
    inputs=read(r/'unified-checkpoint-inputs.json')
    assert inputs['unifiedEntrySha256']==sha(w/'eng/release-check.ps1')
    for row in inputs['inputs']:assert sha(Path(row['path']))==row['sha256']
    callers=read(r/'manual-caller-equivalence.json')
    assert callers['state']=='PASS' and callers['manualToEmitted']==12 and callers['configurationExact'] and len(callers['callers'])==8
    for row in callers['callers']:assert sha(w/row['path'])==row['sha256']
    for configuration in ('release','debug'):
        verify_native_probe((r/f'adapter-{configuration}.log').read_text(encoding='utf-8-sig'),'Topology face adapters equivalence PASS:12 exact SDK calls')
        verify_native_probe((r/f'typed-buffers-{configuration}.log').read_text(encoding='utf-8-sig'),'Typed topology C ABI PASS:')
    for context in ('actual-debug-visible','final-visible','facade-visible','module-visible'):
        result=read(r/context/'visible-result.json')
        assert result['state']=='PASS' and result['pixels']>100 and result['ringHit'] and result['holeCentreMiss'] and result['visibilityStale']
        assert result['presentation']==result['actualSelectedIdentity'] and result['stepArea']==12
        assert 'DXGI' in result['capture'] and 'HWND' in result['input']
        for name in ('before.png','selected.png','holed.step'):assert (r/context/name).is_file()
        for name in ('before.png','selected.png'):
            verify_fixture_frame((r/context/name).read_bytes(),(r/'focused-visible-3'/name).read_bytes())
if a.batch == 'bl':
    for configuration in ('release','debug'):
        verify_native_probe((r/f'parameterized-buffers-{configuration}.log').read_text(encoding='utf-8-sig'),'Parameterized construction C ABI PASS:')
if a.batch == 'bm':
    for configuration in ('release','debug'):
        verify_native_probe((r/f'finite-extrema-buffers-{configuration}.log').read_text(encoding='utf-8-sig'),'Finite extrema C ABI PASS:')
if a.batch == 'bn':
    for configuration in ('release','debug'):
        verify_native_probe((r/f'bounded-text-buffers-{configuration}.log').read_text(encoding='utf-8-sig'),'Bounded text C ABI PASS:')
if a.batch == 'bo':
    for configuration in ('release','debug'):
        verify_native_probe((r/f'mesh-proximity-buffers-{configuration}.log').read_text(encoding='utf-8-sig'),'Mesh proximity C ABI PASS:')
if a.batch in ('bp','bq'):
    for configuration in ('release','debug'):
        verify_native_probe((r/f'local-step-export-buffers-{configuration}.log').read_text(encoding='utf-8-sig'),'Local STEP export C ABI PASS:')
if a.batch in ('bf','bg','bh','bi','bj','bk','bl','bm','bn','bo','bp','bq'):
    verify_final_runtime((r/'runtime-final-package-payloads.log').read_text(encoding='utf-8-sig'), a.runtime)
    for visual in ('actual-debug-visible/visible-result.json','final-visible/visible-result.json'):
        verify_visible_result(read(r/visual))
    if 'admissionPath' in audit:
        assert sha(repo/audit['admissionPath'])==audit['admissionSha256'], 'Admission evidence changed after audit'
if a.batch in ('bk','bl','bm','bn','bo','bp','bq'):
    for context in ('actual-debug','final','facade','module'):
        verify_configured_mesh_result(read(r/f'whole-round-visible/{context}/visible-result.json'))
if a.batch == 'bq':
    subprocess.run([__import__('sys').executable,'-X','utf8',str(w/'eng/verify-bl-bq-chain.py')],check=True)
repaired=set(audit.get('repairedParserHeaders',[]))
if repaired:
    repair=read(r/'parser-repair-result.json')
    assert repair['state']=='PASS' and set(repair['headers'])==repaired
    assert sha(r/'parser-repair-result.json')==audit['parserRepairEvidenceSha256']
assert {x['Header'] for x in full['Failures']}=={x['Header'] for x in base['Failures']}-repaired
old={d['StableId']:d['State'] for d in base['FinalClassification']['Declarations']};new={d['StableId']:d['State'] for d in inventory['Declarations']}
audit=read(r/'source-audit.json')
delta=set(audit['addedStableIds']);manual=set(audit.get('manualStableIds',[]))
converted=set(audit.get('manualToEmittedStableIds',[]))
verify_inventory_delta(old,new,delta,manual,a.delta,a.manual_delta,audit.get('discoveryStates'),converted)
before=read(r/a.baseline/'manifest.json');after=read(w/'generated/manifest.json')
modified={x['path']:x for x in audit.get('modifiedGeneratedFiles',[])}
assert modified.keys()<={f['relativePath'] for f in before['files']}
if modified or converted:
    admission=read(repo/audit['admissionPath'])
    assert sha(repo/audit['admissionPath'])==audit['admissionSha256']
    assert set(modified)=={x['path'] for x in admission.get('affectedGeneratedFiles',[])}
    assert converted=={x['stableId'] for x in admission['declarations'] if x.get('transition')=='ManualToEmitted' and x['decision']=='ADMITTED'}
verify_generated_output_hashes(
    {f['relativePath']:sha(r/a.baseline/'generated'/f['relativePath']) for f in before['files']},
    {f['relativePath']:sha(w/f['relativePath']) for f in after['files']},list(modified.values()))
assert set(after['sourceStableIds'])-set(before['sourceStableIds'])==delta
snapshot=read(w/f'artifacts/packages/package-inputs.{a.version}.json')
assert snapshot['state']=='PASS' and snapshot['packageVersion']==a.version
for entry in snapshot['sourceInputs']:
    assert sha(repo/entry['Path'])==entry['Sha256'],('Package source input changed',entry['Path'])
payload=read(r/'package-payloads.json');managed={x['name']:x['sha256'] for x in payload['managedPayload']}
for name,digest in managed.items():
    actual=sha(w/'tests/OcctSharp.Runtime.Tests/bin/Release/net10.0/win-x64'/name)
    assert actual==digest,name
files=set(subprocess.check_output(['git','-c','core.quotepath=false','ls-files','--cached','--others','--exclude-standard'],cwd=repo,encoding='utf-8').splitlines())
hashes=[dict(path=p,sha256=sha(repo/p)) for p in sorted(files) if (repo/p).is_file()]
(r/'final-source-hashes.json').write_text(json.dumps(hashes,indent=2)+'\n',encoding='utf-8')
evidence=names+['release-check-initial.log','auxiliary.log','package-final.log','metadata-final.log','checksums-final.log',
               'api-current.json','api-diff.json','owned-release.log','owned-debug.log','geometric-release.log','geometric-debug.log','final-source-hashes.json']
if a.batch=='cg':
    evidence+=['manual-caller-equivalence.json','sdk/source-identity.json','fault-source-audit.json','status-private-baseline.txt','runtime-final-package-payloads.log','recovery-frame-reference-review.json','recovery-evidence-fixtures.log']
    evidence+=['unified-checkpoint-result.json','unified-continuation-inputs.json','passed-compiled-inputs.json','release-check-1-bundle-abi.log','release-check-resume.log']
    evidence += [f'{context}/{name}' for context in ('recovery-release-4','recovery-debug','recovery-package') for name in ('result.json','loaded-native.json')]
    evidence += [f'{context}/{row["name"]}{suffix}.png' for context in ('recovery-release-4','recovery-debug','recovery-package') for row in read(r/context/'result.json')['frames'] for suffix in ('','-other')]
if a.batch=='cf':
    evidence+=['manual-caller-equivalence.json','sdk/source-identity.json','wpf-evidence-fixtures.log','status-private-baseline.txt','runtime-final-package-payloads.log','wpf-repro/capture-loss-reproduction.json']
    evidence += [f'{context}/{name}' for context in ('wpf-release-2','wpf-debug-2','wpf-package') for name in ('input-workflow.json','loaded-native.json','capture-loss-reproduction.json','normal.png','zoom.png','pan.png','rotate.png','before-clip.png','clipped.png','restored.png','other-before.png','other-after.png')]
if a.batch=='ce':
    evidence+=['manual-caller-equivalence.json','sdk/source-identity.json','resource-evidence-fixtures.log','status-private-baseline.txt','runtime-final-package-payloads.log']
    evidence += [f'{context}/{name}' for context in ('actual-debug-resources','final-resources','facade-resources','module-resources') for name in ('嵌套-Gltf.json','嵌入-Gltf.json','嵌套-Obj.json','format-chain.json')]
    evidence += [entry.name for entry in r.glob('resource-runtime-focused-*.log')]
    evidence += [entry.name for entry in r.glob('generator-focused*.log')]
if a.batch=='cd':
    evidence+=['manual-caller-equivalence.json','sdk/source-identity.json','fidelity-fixtures.log','status-private-baseline.txt','runtime-final-package-payloads.log','field-fidelity.json']
    evidence+=['unified-checkpoint-inputs.json','unified-checkpoint-result.json','failed-package-inputs.json','passed-compiled-inputs.json','passed-compiled-inputs-2.log','release-check-1-duplicate-docs.log','release-check-resume.log','release-check-resume-1-missing-root.log','release-check-tail-1-missing-counts.log','release-check-tail-2.log','checkpoint-facade-before-doc-fix.csproj','checkpoint-tooling-fixtures.log','checkpoint-tooling-fixtures-2.log','status-preservation-fixtures.log']
    evidence += [f'{context}/{profile}{suffix}' for context in ('actual-debug-fields','final-fields','facade-fields') for profile in ledger['profiles'] for suffix in ('.json','.step')]
    evidence += [f'module-fields/Ap242-Native{suffix}' for suffix in ('.json','.step')]
    evidence += [entry.name for entry in r.glob('runtime-*.log')]
    evidence += [entry.name for entry in r.glob('generator-*.log')]
if a.batch=='cc':
    evidence+=['unified-checkpoint-inputs.json','unified-checkpoint-result.json','release-check-resume.log','release-check-6-unindexed-generated.log','checkpoint-facade.csproj','resume-unified.ps1','complete-unified-checkpoint.py']
    evidence+=['desktop-restored-result.json','desktop-restored-seven.log','desktop-failed-seven-retry.log','release-check-4-desktop-timeout.log','DesktopCapture-before-timeout.cs','desktop-timeout-checkpoint.json','release-check-5-stale-sample-identity.log','SampleProgram-before-bridge-fix.cs','sample-release-build.log','sample-debug-build.log','sample-release-smoke.log','sample-debug-smoke.log']
    evidence+=['adapter-release.log','adapter-debug.log','manual-caller-equivalence.json','sdk/source-identity.json','runtime-existing-focused.log','runtime-workflow-path.log','generator-path-focused.log','runtime-final-package-payloads.log','auxiliary-inputs.json','reference-frame-review.json','tooling-evidence-fixtures.log','status-private-baseline.txt','status-private-preservation.json','receiver-order-build-interruption.json','shared-path-build-interruption.json','release-check-1-old-output-count.log','release-check-2-before-receiver-order.log','release-check-3-before-expanded-paths.log']
    evidence += [f'{context}/{name}' for context in ('inspected-reference','actual-debug-visible','final-visible','facade-visible','module-visible') for name in ('workflow-result.json','oracle.json','rejection-result.json','assembly.step','part-1.stp','part-2.stp','located/visible-result.json','located/before.png','located/selected.png','located/edited.step')]
if a.batch=='cb':
    evidence+=['adapter-release.log','adapter-debug.log','manual-caller-equivalence.json','sdk/source-identity.json',
               'runtime-workflow-focused-final.log','runtime-existing-focused-release.log','generator-focused.log',
               'runtime-final-package-payloads.log','auxiliary-inputs.json','reference-frame-review.json',
               'tooling-evidence-fixtures.log','status-private-baseline.txt','runtime-validation-boundary.json']
    evidence+=['release-check-1-module-fixture.log','release-check-resume.log','unified-checkpoint-inputs.json','unified-checkpoint-result.json',
               'workflow-before-module-fix.cs','runtime-workflow-module-fix.log','module-fixture-build.log','checkpoint-facade.csproj']
    evidence += [entry.name for entry in r.glob('adapter-release-*.log')]
    evidence += [entry.name for entry in r.glob('runtime-workflow-*-analyzer.log')]
    evidence += ['native-release-1-temporary-receiver.log','tooling-evidence-fixtures-1-stale-prethread-input.log']
    evidence += [f'{context}/{format.lower()}/{name}' for context in ('inspected-reference','actual-debug-visible','final-visible','facade-visible','module-visible') for format in ('Binary','Xml') for name in ('workflow-result.json','visible-result.json','before.png','selected.png','edited.step',('document.xml' if format=='Xml' else 'document.xbf'))]
if a.batch=='ca':
    evidence+=['adapter-release.log','adapter-debug.log','manual-caller-equivalence.json','sdk/source-identity.json',
               'runtime-workflow-focused.log','generator-focused-scope.log','runtime-final-package-payloads.log',
               'auxiliary-inputs.json','reference-frame-review.json','tooling-evidence-fixtures.log','status-private-baseline.txt',
               'status-private-preservation.json','runtime-validation-boundary.json','release-check-1-desktop-and-version.log',
               'desktop-recovery-focused.log','workflow-build-1-method-group.log','runtime-workflow-focused-1-root-index.log',
               'release-check-2-bundle-version.log','release-check-resume.log','unified-checkpoint-inputs.json','unified-checkpoint-result.json',
               'release-check-resume-1-sample-version.log','sample-build.log']
    evidence+=['checkpoint-facade.csproj']
    evidence += [f'{context}/{operation.lower()}/{name}' for context in ('inspected-reference','actual-debug-visible','final-visible','facade-visible','module-visible') for operation in ('Remesh','Patch') for name in ('workflow-result.json','visible-result.json','before.png','selected.png','source.xbf',('edited.brep' if operation=='Patch' else 'edited.step'))]
if a.batch=='bz':
    evidence+=['adapter-release.log','adapter-debug.log','manual-caller-equivalence.json','sdk/source-identity.json',
               'runtime-focused.log','runtime-workflow-focused.log','generator-focused.log','runtime-final-package-payloads.log',
               'auxiliary-inputs.json','reference-frame-review.json','runtime-adapter-boundary.json','tooling-evidence-fixtures.log',
               'adapter-release-3.log','adapter-release-4.log','adapter-release-5-sdk-first.log','adapter-probe-4.cpp']
    evidence += [f'{context}/{operation}/{name}' for context in ('inspected-reference','actual-debug-visible','final-visible','facade-visible','module-visible') for operation in ('fillet','chamfer','sweep') for name in ('workflow-result.json','visible-result.json','before.png','selected.png','edited.step')]
if a.batch=='by':
    evidence+=['adapter-release.log','adapter-debug.log','manual-caller-equivalence.json','sdk/source-identity.json','runtime-focused.log','generator-focused.log','runtime-final-package-payloads.log','auxiliary-inputs.json','reference-frame-review.json','history-domain-correction.json','tooling-evidence-fixtures.log','visible-lane-start.json']
    evidence+=['release-check-1-private-fingerprint.log','release-check-resume.log','unified-checkpoint-inputs.json','unified-checkpoint-result.json']
    evidence += [f'{context}/{operation}/{name}' for context in ('inspected-reference','actual-debug-visible','final-visible','facade-visible','module-visible') for operation in ('fuse','cut','common') for name in ('workflow-result.json','visible-result.json','before.png','selected.png','edited.step')]
if a.batch=='bv':
    evidence+=['adapter-release.log','adapter-debug.log','manual-caller-equivalence.json','sdk/source-identity.json',
               'runtime-seam-repro.log','runtime-focused.log','runtime-focused-consumer.log','generator-focused.log',
               'runtime-final-package-payloads.log','tooling-output-fixtures.log','tooling-delta-fixtures.log','tooling-evidence-fixtures.log',
               'boundary-result.json','boundary-probe-runtime.log','auxiliary-inputs.json']
if a.batch=='bw':
    evidence+=['adapter-release.log','adapter-debug.log','manual-caller-equivalence.json','sdk/source-identity.json',
               'runtime-focused-5.log','runtime-focused-debug.log','generator-focused.log','runtime-final-package-payloads.log',
               'filling-buffers-release.log','filling-buffers-debug.log',
               'two-section-sdk-release.log','two-section-sdk-debug.log',
               'appsweep-readonly-release.log','appsweep-readonly-debug.log','auxiliary-inputs.json',
               'tooling-output-fixtures.log','tooling-delta-fixtures.log','tooling-evidence-fixtures.log',
               'legacy-baseline-result.json','legacy-baseline-runtime.log','legacy-baseline-build.log',
               'release-check-1-legacy-fixture.log','release-check-resume.log',
               'unified-checkpoint-inputs.json','unified-checkpoint-result.json']
if a.batch=='bx':
    evidence+=['adapter-release.log','adapter-debug.log','typed-buffers-release.log','typed-buffers-debug.log',
               'manual-caller-equivalence.json','sdk/source-identity.json','runtime-focused.log','generator-focused.log',
               'runtime-viewer-focused-3.log','runtime-final-package-payloads.log','auxiliary-inputs.json',
               'release-check-1-module-fixture.log','release-check-resume.log',
               'unified-checkpoint-inputs.json','unified-checkpoint-result.json',
               'module-fixture-check.log','module-fixture-check-2.log','module-fixture-runtime.log',
               'module-fixture-runtime-2.log','module-fixture-runtime-3.log','tooling-evidence-fixtures.log','visible-lane-start.json',
               'focused-visible-3/before.png','focused-visible-3/selected.png','complete-2-peer-wait.log','fixture-frame-checks.log']
    evidence += ['frame-ready-focused.log','frame-ready-focused-2.log',
                 'frame-ready-focused-2/before.png','frame-ready-focused-2/selected.png',
                 'frame-ready-focused-2/visible-result.json',
                 'integration-actual-debug-1-animation.log',
                 'integration-actual-debug-result-1-animation.json',
                 'integration-debug-inputs-1-animation.json',
                 'integration-debug-loaded-modules-1-animation.json','complete-3-animation.log',
                 'actual-debug-visible-1-animation/before.png',
                 'actual-debug-visible-1-animation/selected.png',
                 'pre-stable-release-check-resume.log','pre-stable-release-check-initial.log',
                 'pre-stable-unified-checkpoint-inputs.json','pre-stable-unified-checkpoint-result.json']
    evidence += ['actual-debug-frame-recapture.json','actual-debug-recapture.log',
                 'recapture-current-actual-debug.ps1','observe-debug.ps1','complete-4-stable.log',
                 'actual-debug-frame-recapture/focused.log','actual-debug-frame-recapture/frame-recapture.trx',
                 'actual-debug-visible-2-routing-overwrite/visible-result.json',
                 'actual-debug-visible-2-routing-overwrite/before.png','actual-debug-visible-2-routing-overwrite/selected.png',
                 'actual-debug-recapture-1-prefix.log','recapture-current-actual-debug-1-prefix.ps1',
                 'actual-debug-frame-recapture-1-prefix/focused.log','actual-debug-frame-recapture-1-prefix/frame-recapture.trx',
                 'actual-debug-recapture-2-report.log','recapture-current-actual-debug-2-report.ps1',
                 'actual-debug-frame-recapture-2-report/focused.log','actual-debug-frame-recapture-2-report/frame-recapture.trx']
    evidence += [f'{context}/{name}' for context in ('actual-debug-visible','final-visible','facade-visible','module-visible') for name in ('visible-result.json','before.png','selected.png','holed.step')]
if a.batch == 'bt':
    evidence += ['runtime-final-package-payloads.log','sdk/source-identity.json','runtime-focused-2.log',
                 'release-check-1-freshness-drift.log','build-release-1.log','generator-contracts.log',
                 'initial-failed-integration-debug-loaded-modules.json','initial-failed-integration-actual-debug.log',
                 'screenshot-serial-verification.json','release-check-tool-completion.json','release-check-process-output.log']
if a.batch == 'bu':
    evidence += ['runtime-final-package-payloads.log','sdk/source-identity.json','runtime-focused-1.log','runtime-focused-2.log',
                 'runtime-nested-enumeration-repro.log','release-check-1-before-enumeration-fix.log',
                 'generator-contracts.log','generator-contracts-1-canonical-model-fixture.log',
                 'manual-caller-audit.json','native-evidence-reuse.json','input-verification.log']
if a.batch in ('bf','bg','bh','bi','bj','bk','bl','bm','bn','bo','bp','bq'):
    evidence += ['runtime-final-package-payloads.log','actual-debug-visible/visible-result.json','final-visible/visible-result.json']
if a.batch in ('bk','bl','bm','bn','bo','bp','bq'):
    evidence += [f'whole-round-visible/{context}/{name}' for context in ('actual-debug','final','facade','module') for name in ('visible-result.json','before.png','selected.png')]
if a.batch == 'bq':
    evidence += ['whole-round-evidence.json','whole-round-evidence-fixtures.log']
    evidence += [f'round-visible/{context}/{name}' for context in ('actual-debug','final','facade','module') for name in ('chain-result.json','visible-result.json','assembly.step','part-1.stp','part-2.stp','roundtrip.xbf','before.png','selected.png')]
if repaired:
    evidence += ['parser-repair-result.json','parser-repair-discovery.json','parser-source-audit.json']
for optional in ['numeric-buffers-release.log','numeric-buffers-debug.log','delta-fixtures.log','version-fixtures.log',
                 'release-check-continuation.log','passed-build-identities.json',
                 'modeling-buffers-release.log','modeling-buffers-debug.log','evidence-fixtures.log',
                 'parameterized-buffers-release.log','parameterized-buffers-debug.log',
                 'finite-extrema-buffers-release.log','finite-extrema-buffers-debug.log',
                 'bounded-text-buffers-release.log','bounded-text-buffers-debug.log',
                 'mesh-proximity-buffers-release.log','mesh-proximity-buffers-debug.log',
                 'local-step-export-buffers-release.log','local-step-export-buffers-debug.log']:
    if (r/optional).is_file(): evidence.append(optional)
report=dict(schemaVersion='1.0',state='PASS',date=datetime.now(timezone(timedelta(hours=9))).date().isoformat(),timezone='Asia/Tokyo',deliveryRepository=str(repo),
    packageVersion=a.version,nativeAbi=a.abi,bridgeVersion=a.bridge,
    admission=dict(generatedDelta=a.delta,manualDelta=a.manual_delta-len(converted),manualToEmitted=len(converted),newGeneratedSdkDeclarations=a.delta-len(converted),admitted=audit['admitted'],deferred=audit['deferred'],newPrerequisites=audit['newPrerequisites']),
    release=dict(generator=a.generator,runtime=a.runtime),debug=dict(generator=a.generator,runtime=a.runtime),
    isolatedActualDebug=a.runtime,actualLoadedDebugDlls=62,cleanSource=dict(generator=a.generator,runtime=a.runtime,byteIdenticalGeneratedFiles=len(after['files'])),
    unchangedBaselineGeneratedFiles=len(before['files'])-len(modified),modifiedBaselineGeneratedFiles=len(modified),nativeExports=dict(count=exports['currentCount'],added=a.exports,removed=0,debugParity=True),
    api=dict(count=len(api['Signatures']),added=diff['AddedCount'],removed=0),
    facadeForwarders=len(re.findall(r'TypeForwardedTo\(typeof\(', (w/'src/OcctSharp/Compatibility/TypeForwarders.Generated.cs').read_text())),
    abi=dict(c11Headers=c11[0],geometricLayouts=34,privateHeaders=read(r/'private-headers/result.json')['headerCount'],releaseRacesPerConfiguration=250,acquisitionsPerConfiguration=16000),
    packages=14,nativeDlls=62,managedPayloads=13,inventoryStates=inventory['DeclarationStates'],inventorySha256=sha(w/'artifacts/generator-reports/full-inventory.json'),
    parserExclusions=len(full['Failures']),repairedParserHeaders=sorted(repaired),newDiscoveryDeclarations=len(audit.get('discoveryStates',{})),
    remainingPlannedBatches=a.remaining,batchImplementationComplete=True,publicReleaseReady=False,
    commitMade=False,pushMade=False,notRun=['Hosted CI','Signing','Push','Public NuGet','Full downstream application acceptance'],
    sourceSnapshotSha256=sha(r/'final-source-hashes.json'),packageInputManifestSha256=sha(w/f'artifacts/packages/package-inputs.{a.version}.json'),frozenInputs=len(snapshot['sourceInputs']),
    releaseBridgeSha256=sha(w/'artifacts/native/Release/OcctSharp.Native.dll'),debugBridgeSha256=debug['debugBridgeSha256'],
    packageHashes=[dict(name=p.name,sha256=sha(p)) for p in sorted((w/'artifacts/packages').glob(f'*.{a.version}.nupkg'))],
    evidence=[dict(path=p,sha256=sha(r/p)) for p in evidence])
assert len(report['packageHashes'])==14
if a.batch=='cg':
    report['viewerRecovery']=dict(actualSdkContextFaultsPerConfiguration=4,pairedFullDesktopFramesPerConfiguration=16,configurations=['Release','actual Debug','final package'],otherViewportPixelDifferences=0,physicalGpuRemoval='NOT RUN',mechanism='Test-only actual SDK window context slot absent or uninitialized; original context parked only for safe teardown before explicit production-host recreation.')
    report['notRun']+=['Physical GPU removal','External CAD interoperability','Mixed DPI/RDP','Human application acceptance']
(r/'delivery-result.json').write_text(json.dumps(report,indent=2)+'\n',encoding='utf-8')
print(json.dumps({k:report[k] for k in ['state','packageVersion','release','debug','api','remainingPlannedBatches']}))
