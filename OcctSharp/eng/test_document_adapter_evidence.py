import copy,json,unittest
from pathlib import Path
from migration_delivery_evidence import verify_document_adapter_result

class DocumentAdapterEvidenceTests(unittest.TestCase):
    def test_actual_positive_each_required_field_and_invalid_geometry(self):
        root=Path(__file__).resolve().parents[1]/'artifacts/batch-cb/focused-visible'
        for format in ('Binary','Xml'):
            workflow=json.loads((root/format.lower()/'workflow-result.json').read_text())
            visible=json.loads((root/format.lower()/'visible-result.json').read_text())
            verify_document_adapter_result(workflow,visible,format)
            for result,is_workflow in ((workflow,True),(visible,False)):
                for key in result:
                    bad=copy.deepcopy(result);bad.pop(key)
                    with self.subTest(format=format,field=key),self.assertRaises(ValueError):
                        verify_document_adapter_result(bad if is_workflow else workflow,visible if is_workflow else bad,format)
                for volume in (float('nan'),float('inf'),25):
                    bad=copy.deepcopy(result);bad['volume' if is_workflow else 'stepVolume']=volume
                    with self.assertRaises(ValueError):
                        verify_document_adapter_result(bad if is_workflow else workflow,visible if is_workflow else bad,format)
            for value in (0,-1,visible['presentation']+1):
                bad=copy.deepcopy(visible);bad['actualSelectedIdentity']=value
                with self.assertRaises(ValueError):verify_document_adapter_result(workflow,bad,format)
            with self.assertRaises(ValueError):verify_document_adapter_result(workflow,visible,'Step')
if __name__=='__main__':unittest.main()
