from pathlib import Path
import hashlib,json,argparse
from whole_round_pixels import rgb,interior_counts
w=Path(__file__).resolve().parents[1]
p=argparse.ArgumentParser();p.add_argument('--root',type=Path,default=w/'artifacts/batch-bq/round-visible');root=p.parse_args().root
read=lambda p:json.loads(p.read_text(encoding='utf-8-sig'))
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest().upper()
for context in ('actual-debug','final','facade','module'):
 r=root/context;chain=read(r/'chain-result.json');view=read(r/'visible-result.json')
 assert chain['state']==view['state']=='PASS'
 assert all(chain[k] is True for k in ('newBlGeometry','bmInputClosed','bmCoupledPointResidual','boInputClosed','exportedHeightVerified'))
 assert abs(chain['exportedHeight']-chain['layerThickness'])<1e-5
 assert chain['finiteSquareDistance']==4 and abs(chain['layerThickness']-2)<1e-6
 assert chain['publicConsumer']==('module' if context=='module' else 'facade')
 assert len({file['FileName'] for file in chain['newBpFiles']})==3
 assert {file['FileName'] for file in chain['newBpFiles']}=={'assembly.step','part-1.stp','part-2.stp'}
 assert len(chain['newBpFiles'])==3 and chain['mainFile']=='assembly.step'
 for file in chain['newBpFiles']:
  p=r/file['FileName'];assert p.stat().st_size==file['Bytes'] and sha(p)==file['Sha256']
 assert sha(r/'assembly.step')==view['sourceStepSha256']
 assert sha(r/'roundtrip.xbf')==view['persistedXbfSha256']
 assert view['persisted'] is True and view['persistedValues']==chain['xbfMetadataValues']
 assert chain['unicodeNames']==['装配 🚀','齿轮·轴','重复实例','标准 部件']==view['persistedValues'][:4]
 assert len(view['persistedValues'])==8
 assert view['presentation']>0 and view['root'] and view['definition']
 metrics=json.loads(view['persistedValues'][4]);assert metrics['finiteSquareDistance']==4 and metrics['layerThickness']==chain['layerThickness']
 assert metrics['finiteRows']==1 and metrics['finiteEndpointCombinations']==4 and metrics['layerNativeDone'] is True
 assert metrics['exportedHeight']==chain['exportedHeight']
 assert metrics['exportedFiles']==chain['newBpFiles'] and metrics['unicodeNames']==chain['unicodeNames']
 assert view['persistedValues'][5:]==[file['FileName']+':'+file['Sha256'] for file in chain['newBpFiles']]
 assert metrics['approximate'] is True and metrics['certifiedBound'] is None
 assert view['pixels']>100 and view['occurrence']!=view['other']
 assert view['visibilityStale'] is True and view['transformStale'] is True
 assert 'DXGI' in view['capture'] and 'HWND' in view['input']
 observed=interior_counts(rgb(r/'before.png'),rgb(r/'selected.png'))
 assert observed==(view['visibleModelPixelsBefore'],view['visibleModelPixelsAfter'],view['interiorSelectionPixels'])
 assert observed[0]>1000 and observed[1]>1000 and observed[2]>100
 assert view['framing']=='axonometric / 0.65 zoom after FitAll'
report=dict(state='PASS',contexts=4,realBlBmBoBnBpChain=True,newFilesPerContext=3,actualXbfValues=True,actualHwndDxgiIdentity=True,visibilityTransformStale=True)
(root.parent/'whole-round-evidence.json').write_text(json.dumps(report,indent=2)+'\n',encoding='utf-8')
print(report)
