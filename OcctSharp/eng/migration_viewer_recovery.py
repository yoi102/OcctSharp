"""Independent full-frame checks for actual SDK-context faults and explicit recreation."""
from pathlib import Path
import json,hashlib
from PIL import Image

def read(path):return json.loads(Path(path).read_text(encoding='utf-8-sig'))
def sha(path):return hashlib.sha256(Path(path).read_bytes()).hexdigest().upper()
def pixels(path):
 with Image.open(path) as image:
  assert image.size==(260,260),path
  return image.convert('RGBA').tobytes('raw','BGRA')
def verify_recovery(root,native,reference,shim=None,managed=None):
 root,native,reference=map(Path,(root,native,reference)); result=read(root/'result.json')
 assert result['state']=='PASS' and result['errors']==0 and result['physicalGpuRemoval']=='NOT RUN'
 assert len(result['faults'])==4 and {(x['cycle'],x['mode']) for x in result['faults']}=={(i,m) for i in range(2) for m in (2,4)}
 for row in result['faults']:
  assert row['independentSdkState']==row['productFlags']==row['mode']
  assert row['oldOwnerDisposed'] and row['retainedSourceFileAbsent']
  assert row['recoveredPixelDifferences']==row['otherPixelDifferences']==0
 assert not (root/'材质源纹理.png').exists()
 names=['baseline','material','texture-source-deleted','reupload']+[f'cycle{i}-fault{m}-{state}' for i in range(2) for m in (2,4) for state in ('before','during','recreated')]
 assert len(result['frames'])==16 and {x['name'] for x in result['frames']}==set(names)
 expected={x['name']:x for x in read(reference/'result.json')['frames']}
 baseline=pixels(reference/'baseline.png'); material=pixels(reference/'material.png'); texture=pixels(reference/'texture-source-deleted.png')
 assert len(set(baseline))>4 and len(set(texture))>15
 assert sum(a!=b for a,b in zip(baseline,material))>1500 and sum(a!=b for a,b in zip(material,texture))>1500
 # The complete model must have visible background at every border, preventing
 # cropped or consistently occluded frames from passing by matching counters.
 for data in (baseline,material,texture):
  border=[data[(y*260+x)*4:(y*260+x)*4+3] for y in range(260) for x in range(260) if x in (0,259) or y in (0,259)]
  assert len(set(border))==1,'Model region touches/crosses capture boundary or is occluded'
 for row in result['frames']:
  name=row['name'];left=pixels(root/(name+'.png'));other=pixels(root/(name+'-other.png'))
  assert row['width']==row['height']==260 and row['leftHwnd']!=row['rightHwnd'] and row['leftHwnd'] and row['rightHwnd']
  assert hashlib.sha256(left).hexdigest().upper()==row['leftHash'] and hashlib.sha256(other).hexdigest().upper()==row['otherHash']
  assert row['leftHash']==expected[name]['leftHash'] and row['otherHash']==expected[name]['otherHash']
  assert other==baseline
  assert left==(baseline if name=='baseline' else material if name=='material' else texture)
 loaded=read(root/'loaded-native.json'); by_name={Path(x['path']).name:x for x in loaded}
 required={x.name for x in native.glob('*.dll')}; assert len(required)==62 and required<=by_name.keys()
 for name in required:
  row=by_name[name];assert sha(row['path'])==row['sha256']==sha(native/name),name
 assert 'ContextFaultShim.dll' in by_name
 if shim:assert sha(by_name['ContextFaultShim.dll']['path'])==by_name['ContextFaultShim.dll']['sha256']==sha(shim)
 assert by_name['ContextFaultShim.dll']['path'] not in [str(native/x) for x in required]
 if managed:
  host=Path(by_name['OcctSharp.Native.dll']['path']).parent.parent
  assert len(managed)==13
  for row in managed:assert sha(host/row['name'])==row['sha256'],('Recovery host managed payload differs',row['name'])
 return dict(state='PASS',actualContextFaults=4,pairedFullDesktopFrames=16,retainedTexturePixels=True,otherViewportPixelDifferences=0,loadedProductDlls=62,testOnlyShimSeparate=True)
