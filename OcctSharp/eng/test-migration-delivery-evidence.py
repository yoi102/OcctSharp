import unittest
from migration_delivery_evidence import verify_final_runtime,verify_visible_result,verify_configured_mesh_result,verify_round_position,verify_native_probe,verify_boolean_repair_result
class EvidenceTests(unittest.TestCase):
 def test_boolean_repair_requires_real_geometry_all_paths_and_hwnd_identity(self):
  for op,volume,solids,inputs in [('Fuse',1088,1,4),('Cut',800,2,3),('Common',200,1,4)]:
   workflow=dict(state='PASS',operation=op,stepVolume=volume,stepSolids=solids,inputs=inputs,argumentCount=2,plan='plan',split=True,deleted=True,shared=True,sourcePreserved=True,foreignRejected=True,staleRejected=True,reopenedIdentityRejected=True,explicitDeletion=True,independentLifetime=True)
   visible=dict(state='PASS',operation=op,stepVolume=volume,pixels=101,bodyHit=True,splitGapMiss=True,visibilityStale=True,presentation=1,actualSelectedIdentity=1,capture='DXGI',input='HWND')
   verify_boolean_repair_result(workflow,visible,op)
   for key,value in [('sourcePreserved',False),('foreignRejected',False),('staleRejected',False),('reopenedIdentityRejected',False),('explicitDeletion',False),('independentLifetime',False),('stepVolume',float('nan')),('stepVolume',volume+1),('stepSolids',solids+1),('inputs',inputs+1)]+([('split',False),('deleted',False)] if op=='Cut' else [('shared',False)] if op=='Common' else []):
    invalid=dict(workflow);invalid[key]=value
    with self.assertRaises(ValueError):verify_boolean_repair_result(invalid,visible,op)
   for key,value in [('pixels',100),('bodyHit',False),('visibilityStale',False),('presentation',0),('actualSelectedIdentity',2),('capture','offscreen'),('input','model'),('stepVolume',float('inf'))]+([('splitGapMiss',False)] if op=='Cut' else []):
    invalid=dict(visible);invalid[key]=value
    with self.assertRaises(ValueError):verify_boolean_repair_result(workflow,invalid,op)
 def test_round_position_cannot_finish_first_batch_or_reopen_completed_last_batch(self):
  self.assertEqual('BL–BQ',verify_round_position('bl',5))
  self.assertEqual('BL–BQ',verify_round_position('bq',0))
  self.assertEqual('BF–BK',verify_round_position('bk',0))
  for index,batch in enumerate(('br','bs','bt','bu','bv','bw','bx','by','bz','ca','cb','cc','cd','ce','cf','cg','ch','ci')):
   self.assertEqual('BR–CI',verify_round_position(batch,17-index))
   with self.assertRaises(ValueError):verify_round_position(batch,18-index)
  for batch,remaining in [('bl',0),('bq',1),('bk',-1)]:
   with self.assertRaises(ValueError):verify_round_position(batch,remaining)
 def test_native_probe_requires_one_actual_success_marker(self):
  marker='Parameterized construction C ABI PASS:'
  verify_native_probe(marker+' canaries passed.',marker)
  for log in ('','compiler failure','prefix '+marker,marker+'\n'+marker):
   with self.assertRaises(ValueError):verify_native_probe(log,marker)
 def test_exact_success(self):
  verify_final_runtime('Passed! - Failed: 0, Passed: 1195, Skipped: 0, Total: 1195',1195)
 def test_missing_failed_or_changed_count(self):
  for log in ('','Failed: 1, Passed: 1194, Skipped: 0, Total: 1195','Failed: 0, Passed: 1194, Skipped: 0, Total: 1194','Failed: 0, Passed: 1194, Skipped: 1, Total: 1195'):
   with self.assertRaises(ValueError):verify_final_runtime(log,1195)
 def test_duplicate_evidence(self):
  with self.assertRaises(ValueError):verify_final_runtime('Failed: 0, Passed: 1195, Skipped: 0, Total: 1195\n'*2,1195)
 def test_real_visible_success(self):verify_visible_result(self.result())
 def test_visual_and_identity_gates_fail_closed(self):
  for key,value in [('pixels',100),('state','FAIL'),('occurrence',''),('visibilityStale',False),('transformStale',False),('capture','offscreen'),('input','model')]:
   result=self.result();result[key]=value
   with self.assertRaises(ValueError):verify_visible_result(result)
 def test_configured_mesh_requires_actual_saved_source_and_both_occurrences(self):
  result=dict(self.result(),root='0:1:1:1',other='0:1:1:2:2',definition='0:1:1:3',presentation=1,persisted=True,sourceGltfSha256='A'*64)
  verify_configured_mesh_result(result)
  for key,value in [('root',''),('other',result['occurrence']),('definition',''),('presentation',0),('persisted',False),('sourceGltfSha256','stale')]:
   invalid=dict(result);invalid[key]=value
   with self.assertRaises(ValueError):verify_configured_mesh_result(invalid)
 def test_old_visible_evidence_cannot_complete_configured_mesh(self):
  with self.assertRaises(ValueError):verify_configured_mesh_result(self.result())
 def result(self):return dict(state='PASS',pixels=101,occurrence='0:1:1:2:1',visibilityStale=True,transformStale=True,capture='DXGI composited client pixels',input='HWND -> ViewerInputController')
unittest.main()
