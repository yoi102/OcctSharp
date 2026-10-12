import copy,json,tempfile,shutil,unittest
from pathlib import Path
from migration_viewer_recovery import verify_recovery
w=Path(__file__).resolve().parents[1];reference=w/'artifacts/batch-cg/recovery-release-4';native=w/'artifacts/native/Release'
class RecoveryEvidenceTests(unittest.TestCase):
 def test_valid_actual_full_frames(self):verify_recovery(reference,native,reference)
 def reject(self,mutate):
  with tempfile.TemporaryDirectory(prefix='cg-evidence-') as tmp:
   root=Path(tmp);shutil.copytree(reference,root,dirs_exist_ok=True);p=root/'result.json';value=json.loads(p.read_text());mutate(value,root);p.write_text(json.dumps(value))
   with self.assertRaises((AssertionError,FileNotFoundError)):verify_recovery(root,native,reference)
 def test_bad_mode(self):self.reject(lambda x,r:x['faults'][0].update(productFlags=1))
 def test_missing_cycle(self):self.reject(lambda x,r:x['faults'].pop())
 def test_stale_source(self):self.reject(lambda x,r:(r/'材质源纹理.png').write_bytes(b'source'))
 def test_forged_pixel_counter(self):self.reject(lambda x,r:shutil.copy2(r/'material.png',r/'cycle1-fault4-recreated.png'))
 def test_affected_other(self):self.reject(lambda x,r:shutil.copy2(r/'material.png',r/'cycle0-fault2-during-other.png'))
 def test_no_disposal(self):self.reject(lambda x,r:x['faults'][1].update(oldOwnerDisposed=False))
 def test_one_hwnd(self):self.reject(lambda x,r:x['frames'][0].update(leftHwnd=x['frames'][0]['rightHwnd']))
 def test_omitted_frame(self):self.reject(lambda x,r:x['frames'].pop())
 def test_forged_consistent_hashes(self):
  def mutate(x,r):
   from PIL import Image
   import hashlib
   raw=bytes([255])*260*260*4;hash=hashlib.sha256(raw).hexdigest().upper()
   for row in x['frames']:
    row['leftHash']=row['otherHash']=hash
    for suffix in ('','-other'):Image.new('RGBA',(260,260),'white').save(r/(row['name']+suffix+'.png'))
  self.reject(mutate)
if __name__=='__main__':unittest.main()
