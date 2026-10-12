from pathlib import Path
import json,shutil,tempfile,unittest
from migration_wpf_input import verify_wpf_input

w=Path(__file__).resolve().parents[1];source=w/'artifacts/batch-cf/wpf-release-2';native=w/'artifacts/native/Release'
class EvidenceTests(unittest.TestCase):
    def setUp(self):self.temp=tempfile.TemporaryDirectory();self.root=Path(self.temp.name)/'evidence';shutil.copytree(source,self.root)
    def tearDown(self):self.temp.cleanup()
    def change(self,filename,mutate):
        p=self.root/filename;value=json.loads(p.read_text());mutate(value);p.write_text(json.dumps(value))
    def test_actual_evidence(self):verify_wpf_input(self.root,native)
    def test_reject_overlay_even_with_consistent_counters(self):
        from PIL import Image
        for n,name in enumerate(('normal','zoom','pan','rotate','before-clip','clipped','restored')):
            Image.new('RGB',(260,260),(200,n*20,100)).save(self.root/(name+'.png'))
        self.change('input-workflow.json',lambda r:r.update(zoomPixels=67600,panPixels=67600,rotationPixels=67600,clipPixels=67600,clipRestorePixels=67600))
        with self.assertRaises(AssertionError):verify_wpf_input(self.root,native,source)
    def test_reject_mismatched_pixel_counter(self):
        self.change('input-workflow.json',lambda r:r.update(panPixels=0))
        with self.assertRaises(AssertionError):verify_wpf_input(self.root,native)
    def test_reject_fake_capture_success(self):
        self.change('capture-loss-reproduction.json',lambda r:r.update(staleClick=True))
        with self.assertRaises(AssertionError):verify_wpf_input(self.root,native)
    def test_reject_unrelated_hwnd(self):
        self.change('input-workflow.json',lambda r:r['events'][0].update(hwnd=0))
        with self.assertRaises(AssertionError):verify_wpf_input(self.root,native)
    def test_reject_native_hash(self):
        self.change('loaded-native.json',lambda r:r[0].update(sha256='0'*64))
        with self.assertRaises(AssertionError):verify_wpf_input(self.root,native)
    def test_reject_blank_frame(self):
        from PIL import Image
        Image.new('RGB',(260,260)).save(self.root/'normal.png')
        with self.assertRaises(AssertionError):verify_wpf_input(self.root,native)
    def test_reject_missing_cancellation(self):
        self.change('input-workflow.json',lambda r:r.update(rectangleCancel=False))
        with self.assertRaises(AssertionError):verify_wpf_input(self.root,native)
    def test_reject_missing_drag_route(self):
        self.change('input-workflow.json',lambda r:r.update(events=[e for e in r['events'] if e['message']!=0x207]))
        with self.assertRaises(AssertionError):verify_wpf_input(self.root,native)
unittest.main(verbosity=2)
