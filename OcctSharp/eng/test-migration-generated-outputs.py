import unittest
from migration_generated_outputs import verify_generated_output_hashes
class OutputTests(unittest.TestCase):
    def test_preserves_others_and_records_explicit_change(self):
        same,changed=verify_generated_output_hashes({'a':'old','b':'same'},{'a':'new','b':'same','c':'extra'},[dict(path='a',baselineSha256='old',reason='Audited rule')])
        self.assertEqual(same,[dict(path='b',sha256='same')]);self.assertEqual(changed[0]['sha256'],'new')
    def test_unknown_removed_and_unaudited_change_fail(self):
        for old,new,rows in [({'a':'old'},{},[]),({'a':'old'},{'a':'new'},[]),({'a':'old'},{'a':'old'},[dict(path='b',baselineSha256='old',reason='x')])]:
            with self.assertRaises(ValueError):verify_generated_output_hashes(old,new,rows)
    def test_duplicate_bad_baseline_blank_reason_and_unmodified_exception_fail(self):
        row=dict(path='a',baselineSha256='old',reason='x')
        for rows,new in [([row,row],'new'),([dict(row,baselineSha256='bad')],'new'),([dict(row,reason=' ')],'new'),([row],'old')]:
            with self.assertRaises(ValueError):verify_generated_output_hashes({'a':'old'},{'a':new},rows)
    def test_final_audited_changed_bytes_cannot_drift(self):
        row=dict(path='a',baselineSha256='old',sha256='audited',reason='x')
        verify_generated_output_hashes({'a':'old'},{'a':'audited'},[row])
        with self.assertRaises(ValueError):verify_generated_output_hashes({'a':'old'},{'a':'later'},[row])
if __name__=='__main__':unittest.main()
