"""Small fail-closed validators shared by final migration freeze and fixtures."""
import re
import math

def verify_located_assembly_result(workflow,visible,oracle,rejections):
    required=('sourceUnchanged','sourceDeleted','readbackEpochRejected','sharedDefinitionColor',
              'sharedDefinitionLayer','independentLeafLifetime','noncommutingBounds')
    if not(workflow.get('state')=='PASS' and all(workflow.get(k) is True for k in required)
           and workflow.get('roots')==2 and workflow.get('exactNestedSharedComponents')==7
           and workflow.get('distinctComponentLabels')==5 and workflow.get('leafCount')==5
           and workflow.get('sourceLengthUnit')==.001
           and workflow.get('canonicalFiles')==['assembly.step','part-1.stp','part-2.stp']):
        raise ValueError('Located assembly identity, metadata, source/epoch or lifetime proof is incomplete.')
    if not(oracle.get('state')=='PASS' and oracle.get('sdkCloneLostPlacement') is True
           and oracle.get('normalizedLeaves')==5 and oracle.get('sourceUnchanged') is True
           and oracle.get('sharedDefinitions')==2 and len(oracle.get('roots',[]))==2
           and len(oracle.get('leaves',[]))==5):
        raise ValueError('Independent located definition oracle is incomplete.')
    def same(a,b):
        return isinstance(a,(int,float)) and not isinstance(a,bool) and math.isfinite(a) and math.isclose(a,b,rel_tol=0,abs_tol=1e-5)
    expected=sorted(oracle['leaves'],key=lambda x:(x['bounds'][0],x['bounds'][1]))
    bounds=workflow.get('leafBounds',[]);volumes=workflow.get('leafVolumes',[])
    if len(bounds)!=5 or len(volumes)!=5 or not same(workflow.get('volume'),84):
        raise ValueError('Cumulative geometry counts/volume differ.')
    for i,row in enumerate(expected):
        if len(bounds[i])!=6 or len(row['bounds'])!=6 or not same(volumes[i],row['volume']) or not all(same(a,b) for a,b in zip(bounds[i],row['bounds'])):
            raise ValueError('Cumulative noncommuting leaf volume/bounds differ.')
    if not(rejections.get('state')=='PASS' and all(rejections.get(k) is True for k in ('empty','duplicate','occurrence','overlap','openCommand','noPartialOwner','completePathDepth','expandedOccurrences'))):
        raise ValueError('Located graph selection or command rejection proof is incomplete.')
    if not(visible.get('state')=='PASS' and visible.get('operation')=='Located'
           and visible.get('pixels',0)>100 and visible.get('bodyHit') is True
           and visible.get('visibilityStale') is True and visible.get('wrongThreadRejected') is True
           and isinstance(visible.get('presentation'),int) and visible['presentation']>0
           and visible['presentation']==visible.get('actualSelectedIdentity')
           and (same(visible.get('stepVolume'),24) or same(visible.get('stepVolume'),6))
           and 'DXGI' in visible.get('capture','') and 'HWND' in visible.get('input','')):
        raise ValueError('Located imported leaf HWND identity/input/invalidation proof is incomplete.')
def verify_document_adapter_result(workflow,visible,format):
    if format not in ('Binary','Xml'):raise ValueError('Unknown document format.')
    flags=('moduleProjection','unicode','spaceKeys','nullEmptyBounds','exactRealBits',
           'transactionAbort','undoRedo','failureAtomic','foreignContextRejected',
           'closedContextRejected','parentLifetimeRejected','detachedData','referenceIdentity',
           'applicationLocalDrivers','sourceGeometryPreserved')
    if not(workflow.get('state')=='PASS' and workflow.get('format')==format
           and workflow.get('namedCategories')==6 and workflow.get('directFamilies')==3
           and all(workflow.get(k) is True for k in flags)):
        raise ValueError('Document transactions, persistence, context or lifetime proof is incomplete.')
    for result,key in ((workflow,'volume'),(visible,'stepVolume')):
        actual=result.get(key)
        if not isinstance(actual,(int,float)) or not math.isfinite(actual) or not math.isclose(actual,24,rel_tol=0,abs_tol=1e-6):
            raise ValueError('Document exact stored/reopened geometry differs.')
    if not(visible.get('state')=='PASS' and visible.get('operation')==format
           and visible.get('pixels',0)>100 and visible.get('bodyHit') is True
           and visible.get('visibilityStale') is True and visible.get('wrongThreadRejected') is True
           and isinstance(visible.get('presentation'),int) and visible['presentation']>0
           and visible['presentation']==visible.get('actualSelectedIdentity')
           and 'DXGI' in visible.get('capture','') and 'HWND' in visible.get('input','')):
        raise ValueError('Document actual HWND identity, selection, thread or invalidation proof is incomplete.')


def verify_mesh_adapter_result(workflow, visible, operation):
    if operation not in ('Remesh','Patch'):raise ValueError('Unknown mesh operation.')
    flags=('sourceGeometryPreserved','sourceMeshPreserved','sourceDocumentPreserved',
           'foreignRevisionRejected','invalidParameterRejected','failureAtomic',
           'independentLifetime','optionalChannelsPreserved','qualityDetached',
           'orientationTransformPreserved','degenerationHandled','incompleteRootRejected')
    volume,solids=(24,1) if operation=='Remesh' else (0,0)
    if not(workflow.get('state')=='PASS' and workflow.get('operation')==operation
           and all(workflow.get(k) is True for k in flags)
           and workflow.get('stepSolids')==solids and workflow.get('meshTriangles')==2
           and workflow.get('boundaryEdges')==4 and workflow.get('xbfVolume')==24):
        raise ValueError('Mesh source, channel, revision, rollback or lifetime proof is incomplete.')
    for result in (workflow,visible):
        actual=result.get('stepVolume')
        if not isinstance(actual,(float,int)) or not math.isfinite(actual) or not math.isclose(actual,volume,rel_tol=0,abs_tol=1e-5):
            raise ValueError('Mesh exact STEP/discrete-domain volume differs.')
    if not(visible.get('state')=='PASS' and visible.get('operation')==operation
           and visible.get('pixels',0)>100 and visible.get('bodyHit') is True
           and visible.get('visibilityStale') is True and isinstance(visible.get('presentation'),int)
           and visible['presentation']>0 and visible['presentation']==visible.get('actualSelectedIdentity')
           and 'DXGI' in visible.get('capture','') and 'HWND' in visible.get('input','')):
        raise ValueError('Mesh actual HWND identity, selection or invalidation proof is incomplete.')

def verify_local_feature_result(workflow, visible, operation):
    expected={'Fillet':1800-15*(1-math.pi/4),'Chamfer':1792.5,'Sweep':20}
    if operation not in expected:raise ValueError('Unknown local feature operation.')
    flags=('sourcePreserved','foreignRejected','staleRejected','invalidParameterRejected',
           'failureRollback','reopenedIdentityRejected','independentLifetime')
    if not (workflow.get('state')=='PASS' and workflow.get('operation')==operation
            and workflow.get('plan') and isinstance(workflow.get('history'),int) and workflow['history']>0
            and isinstance(workflow.get('simulated'),int) and workflow['simulated']>0
            and workflow.get('stepSolids')==1 and all(workflow.get(k) is True for k in flags)):
        raise ValueError('Local feature source, rollback, simulation, history or lifetime proof is incomplete.')
    for result in (workflow,visible):
        actual=result.get('stepVolume')
        if not isinstance(actual,(float,int)) or not math.isfinite(actual) or not math.isclose(actual,expected[operation],rel_tol=0,abs_tol=1e-5):
            raise ValueError('Local feature reopened STEP volume differs.')
    if not (visible.get('state')=='PASS' and visible.get('operation')==operation
            and visible.get('pixels',0)>100 and visible.get('bodyHit') is True
            and visible.get('visibilityStale') is True and isinstance(visible.get('presentation'),int)
            and visible['presentation']>0 and visible['presentation']==visible.get('actualSelectedIdentity')
            and 'DXGI' in visible.get('capture','') and 'HWND' in visible.get('input','')):
        raise ValueError('Local feature actual HWND selection, identity or invalidation proof is incomplete.')

def verify_boolean_repair_result(workflow, visible, operation):
    expected={'Fuse':(1088,1,4),'Cut':(800,2,3),'Common':(200,1,4)}
    if operation not in expected:raise ValueError('Unknown Boolean operation.')
    volume,solids,inputs=expected[operation]
    flags=('sourcePreserved','foreignRejected','staleRejected','reopenedIdentityRejected','explicitDeletion','independentLifetime')
    if not (workflow.get('state')=='PASS' and workflow.get('operation')==operation
            and workflow.get('argumentCount')==2 and workflow.get('inputs')==inputs
            and workflow.get('stepSolids')==solids and workflow.get('plan')
            and all(workflow.get(key) is True for key in flags)):
        raise ValueError('Boolean repair lifetime, provenance, explicit edit or identity evidence is incomplete.')
    if operation=='Cut' and not(workflow.get('split') is True and workflow.get('deleted') is True):
        raise ValueError('Cut must preserve actual split and deleted history.')
    if operation=='Common' and workflow.get('shared') is not True:
        raise ValueError('Common must preserve every shared target source.')
    for result in (workflow,visible):
        actual=result.get('stepVolume')
        if not isinstance(actual,(float,int)) or not math.isfinite(actual) or not math.isclose(actual,volume,rel_tol=0,abs_tol=1e-5):
            raise ValueError('Actual reopened STEP volume differs.')
    if not (visible.get('state')=='PASS' and visible.get('operation')==operation
            and visible.get('pixels',0)>100 and visible.get('bodyHit') is True
            and visible.get('visibilityStale') is True
            and isinstance(visible.get('presentation'),int) and visible['presentation']>0
            and visible['presentation']==visible.get('actualSelectedIdentity')
            and 'DXGI' in visible.get('capture','') and 'HWND' in visible.get('input','')
            and (operation!='Cut' or visible.get('splitGapMiss') is True)):
        raise ValueError('Boolean actual HWND pixels, pick identity or invalidation evidence is incomplete.')

def verify_round_position(batch, remaining):
    for batches in (('bf','bg','bh','bi','bj','bk'),('bl','bm','bn','bo','bp','bq'),
                    ('br','bs','bt','bu','bv','bw','bx','by','bz','ca','cb','cc','cd','ce','cf','cg','ch','ci')):
        if batch.lower() in batches:
            if remaining != len(batches)-1-batches.index(batch.lower()):
                raise ValueError('Remaining batches disagree with the accepted ordered round.')
            return batches[0].upper()+'–'+batches[-1].upper()
    return None

def verify_native_probe(log, marker):
    if len(re.findall(r'^'+re.escape(marker)+r'[^\r\n]*$',log,re.M)) != 1:
        raise ValueError('Native probe success is missing or duplicated.')

def verify_final_runtime(log, expected):
    pattern = r'Failed:\s+0, Passed:\s+' + str(expected) + r', Skipped:\s+0, Total:\s+' + str(expected)
    if len(re.findall(pattern, log)) != 1:
        raise ValueError('Final packaged-payload runtime suite is missing, failed, duplicated or has a different count.')

def verify_visible_result(result):
    if not (result.get('state') == 'PASS' and result.get('pixels', 0) > 100
            and result.get('occurrence') and result.get('visibilityStale') is True
            and result.get('transformStale') is True and 'DXGI' in result.get('capture', '')
            and 'HWND' in result.get('input', '')):
        raise ValueError('Actual HWND/DXGI, occurrence identity or stale-query assertions did not pass.')

def verify_configured_mesh_result(result):
    verify_visible_result(result)
    if not (result.get('persisted') is True and result.get('root')
            and result.get('definition') and result.get('other')
            and result['occurrence'] != result['other']
            and isinstance(result.get('presentation'), int) and result['presentation'] > 0
            and re.fullmatch(r'[A-F0-9]{64}', result.get('sourceGltfSha256', ''))):
        raise ValueError('Configured mesh saved source, shared definition or actual presentation identity proof is incomplete.')
