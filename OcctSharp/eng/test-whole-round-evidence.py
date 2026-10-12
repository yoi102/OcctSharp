"""Synthetic fixtures exercise rejection; they provide no Viewer acceptance."""
from pathlib import Path
import hashlib,json,subprocess,sys,tempfile,unittest,struct,zlib
from whole_round_pixels import interior_counts
verifier=Path(__file__).with_name('verify-bl-bq-chain.py')
def image_bytes(selected=False):
 data=bytearray(bytes([80,100,120])*360*360)
 for y in range(120,240):
  for x in range(100,260):data[(y*360+x)*3:(y*360+x)*3+3]=bytes([250,250,250] if selected else [120,150,180])
 return bytes(data)
def png(data):
 def chunk(kind,body):return struct.pack('>I',len(body))+kind+body+struct.pack('>I',zlib.crc32(kind+body))
 rows=b''.join(b'\0'+data[y*1080:(y+1)*1080] for y in range(360))
 return b'\x89PNG\r\n\x1a\n'+chunk(b'IHDR',struct.pack('>IIBBBBB',360,360,8,2,0,0,0))+chunk(b'IDAT',zlib.compress(rows))+chunk(b'IEND',b'')
class WholeRoundEvidenceTests(unittest.TestCase):
 def fixture(self,root):
  names=['装配 🚀','齿轮·轴','重复实例','标准 部件']
  for context in ('actual-debug','final','facade','module'):
   r=root/context;r.mkdir();files=[]
   for name in ('assembly.step','part-1.stp','part-2.stp'):
    data=(context+name).encode();(r/name).write_bytes(data);files.append(dict(FileName=name,Bytes=len(data),Sha256=hashlib.sha256(data).hexdigest().upper(),SourceDefinitionEntry='exact'))
   (r/'roundtrip.xbf').write_bytes(b'synthetic fixture')
   before,after=image_bytes(),image_bytes(True)
   for name,data in [('before.png',before),('selected.png',after)]:(r/name).write_bytes(png(data))
   values=names+[json.dumps(dict(finiteSquareDistance=4,layerThickness=2,exportedHeight=2,finiteRows=1,finiteEndpointCombinations=4,layerNativeDone=True,approximate=True,certifiedBound=None,exportedFiles=files,unicodeNames=names))]+[f['FileName']+':'+f['Sha256'] for f in files]
   chain=dict(state='PASS',newBlGeometry=True,bmInputClosed=True,bmCoupledPointResidual=True,boInputClosed=True,exportedHeightVerified=True,exportedHeight=2,finiteSquareDistance=4,layerThickness=2,publicConsumer='module' if context=='module' else 'facade',newBpFiles=files,mainFile='assembly.step',unicodeNames=names,xbfMetadataValues=values)
   view=dict(state='PASS',sourceStepSha256=files[0]['Sha256'],persistedXbfSha256=hashlib.sha256(b'synthetic fixture').hexdigest().upper(),persisted=True,persistedValues=values,presentation=1,root='root',definition='def',occurrence='one',other='two',pixels=101,visibilityStale=True,transformStale=True,capture='DXGI',input='HWND')
   counts=interior_counts(before,after);view.update(visibleModelPixelsBefore=counts[0],visibleModelPixelsAfter=counts[1],interiorSelectionPixels=counts[2],framing='axonometric / 0.65 zoom after FitAll')
   for name,data in [('chain-result.json',chain),('visible-result.json',view)]:(r/name).write_text(json.dumps(data),encoding='utf-8')
 def run_fixture(self,change=None):
  with tempfile.TemporaryDirectory() as directory:
   root=Path(directory)/'round-visible';root.mkdir();self.fixture(root)
   if change:change(root)
   return subprocess.run([sys.executable,'-X','utf8',str(verifier),'--root',str(root)],capture_output=True).returncode
 def test_complete_synthetic_fixture(self):self.assertEqual(0,self.run_fixture())
 def test_changed_actual_file_rejects_manifest(self):self.assertNotEqual(0,self.run_fixture(lambda r:(r/'module/part-1.stp').write_bytes(b'changed')))
 def test_missing_fourth_context_rejects(self):self.assertNotEqual(0,self.run_fixture(lambda r:(r/'actual-debug/chain-result.json').unlink()))
 def test_blank_client_with_border_change_cannot_pass(self):
  def change(root):
   for name in ('before.png','selected.png'):(root/'facade'/name).write_bytes(png(bytes([255,255,255])*360*360))
  self.assertNotEqual(0,self.run_fixture(change))
 def test_stale_identity_or_uncoupled_metadata_rejects(self):
  for key,value in [('pixels',100),('persistedValues',[]),('transformStale',False),('sourceStepSha256','A'*64)]:
   def change(root):
    p=root/'facade/visible-result.json';data=json.loads(p.read_text());data[key]=value;p.write_text(json.dumps(data))
   self.assertNotEqual(0,self.run_fixture(change),key)
unittest.main()
