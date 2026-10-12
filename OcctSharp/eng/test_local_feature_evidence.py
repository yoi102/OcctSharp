import copy,json,unittest
from pathlib import Path
from migration_delivery_evidence import verify_local_feature_result

class LocalFeatureEvidenceTests(unittest.TestCase):
    def test_actual_positive_and_each_required_negative(self):
        root=Path(__file__).resolve().parents[1]/'artifacts/batch-bz/focused-visible'
        for operation in ('Fillet','Chamfer','Sweep'):
            workflow=json.loads((root/operation.lower()/'workflow-result.json').read_text())
            visible=json.loads((root/operation.lower()/'visible-result.json').read_text())
            verify_local_feature_result(workflow,visible,operation)
            for key in workflow:
                bad=copy.deepcopy(workflow);bad.pop(key)
                with self.subTest(operation=operation,field=key),self.assertRaises(ValueError):
                    verify_local_feature_result(bad,visible,operation)
            for key in visible:
                bad=copy.deepcopy(visible);bad.pop(key)
                with self.subTest(operation=operation,field=key),self.assertRaises(ValueError):
                    verify_local_feature_result(workflow,bad,operation)
            for result,is_workflow in ((workflow,True),(visible,False)):
                for volume in (0,float('nan'),float('inf'),result['stepVolume']+1):
                    bad=copy.deepcopy(result);bad['stepVolume']=volume
                    with self.assertRaises(ValueError):
                        verify_local_feature_result(bad if is_workflow else workflow,visible if is_workflow else bad,operation)

if __name__=='__main__':unittest.main()
