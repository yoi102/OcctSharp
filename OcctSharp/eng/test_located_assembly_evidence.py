import copy,json,unittest
from pathlib import Path
from migration_delivery_evidence import verify_located_assembly_result
class LocatedAssemblyEvidenceTests(unittest.TestCase):
 def test_actual_positive_and_each_required_field_and_bad_geometry(self):
  r=Path(__file__).resolve().parents[1]/'artifacts/batch-cc/focused-path-visible'
  inputs=[json.loads(p.read_text()) for p in (r/'workflow-result.json',r/'located/visible-result.json',r/'oracle.json',r/'rejection-result.json')]
  verify_located_assembly_result(*inputs)
  for i,result in enumerate(inputs):
   for key in result:
    bad=copy.deepcopy(inputs);bad[i].pop(key)
    with self.subTest(input=i,field=key),self.assertRaises(ValueError):verify_located_assembly_result(*bad)
  for value in (float('nan'),float('inf'),85):
   bad=copy.deepcopy(inputs);bad[0]['volume']=value
   with self.assertRaises(ValueError):verify_located_assembly_result(*bad)
  for value in (float('nan'),float('inf'),100):
   bad=copy.deepcopy(inputs);bad[0]['leafBounds'][0][0]=value
   with self.assertRaises(ValueError):verify_located_assembly_result(*bad)
  bad=copy.deepcopy(inputs);bad[1]['actualSelectedIdentity']+=1
  with self.assertRaises(ValueError):verify_located_assembly_result(*bad)
if __name__=='__main__':unittest.main()
