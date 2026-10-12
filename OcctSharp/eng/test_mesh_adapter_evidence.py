import copy,json,unittest
from pathlib import Path
from migration_delivery_evidence import verify_mesh_adapter_result

class MeshAdapterEvidenceTests(unittest.TestCase):
    def test_actual_positive_and_each_required_negative(self):
        root=Path(__file__).resolve().parents[1]/'artifacts/batch-ca/focused-visible'
        for operation in ('Remesh','Patch'):
            workflow=json.loads((root/operation.lower()/'workflow-result.json').read_text())
            visible=json.loads((root/operation.lower()/'visible-result.json').read_text())
            verify_mesh_adapter_result(workflow,visible,operation)
            for result,is_workflow in ((workflow,True),(visible,False)):
                for key in result:
                    bad=copy.deepcopy(result);bad.pop(key)
                    with self.subTest(operation=operation,field=key),self.assertRaises(ValueError):
                        verify_mesh_adapter_result(bad if is_workflow else workflow,visible if is_workflow else bad,operation)
                for volume in (float('nan'),float('inf'),result['stepVolume']+1):
                    bad=copy.deepcopy(result);bad['stepVolume']=volume
                    with self.assertRaises(ValueError):
                        verify_mesh_adapter_result(bad if is_workflow else workflow,visible if is_workflow else bad,operation)

if __name__=='__main__':unittest.main()
