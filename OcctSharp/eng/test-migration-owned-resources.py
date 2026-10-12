import copy,json,tempfile
from pathlib import Path
from migration_owned_resources import verify_resource_reports
w=Path(__file__).resolve().parents[1];source=w/'artifacts/batch-ce/resource-runtime';fixture=w/'tests/Fixtures/OwnedMeshResources'
verify_resource_reports(source,fixture)
checks=0
with tempfile.TemporaryDirectory(prefix='OcctSharp.CE.Evidence.') as temporary:
    root=Path(temporary);base={x.name:json.loads(x.read_text(encoding='utf-8-sig')) for x in source.glob('*.json')}
    def save(values):
        for name,value in values.items():(root/name).write_text(json.dumps(value),encoding='utf8')
    for name in ('嵌套-Gltf.json','嵌入-Gltf.json','嵌套-Obj.json'):
        mutations=[lambda x:x.update(state='FAIL'),lambda x:x.update(hash='0'*64),lambda x:x.update(sourceDeleted=False),lambda x:x.update(documentAliasSurvived=False),lambda x:x.update(resultDisposed=False),lambda x:x.update(shapeSurvived=False),lambda x:x['texture'].update(Width=3),lambda x:x['texture'].update(Slot=5),lambda x:x['texture']['EncodedPng'].__setitem__(20,1),lambda x:x['texture']['Corners'][0].update(Red=0),lambda x:x['texture']['Corners'][0].update(Alpha=0),lambda x:x['resources'][0].update(Sha256='0'*64),lambda x:x['resources'][0].update(Bytes=1),lambda x:x['resources'][0].update(FileName='other')]
        for mutate in mutations:
            values=copy.deepcopy(base);mutate(values[name]);save(values)
            try:verify_resource_reports(root,fixture)
            except (AssertionError,KeyError):checks+=1
            else:raise AssertionError('Invalid resource evidence accepted')
    for field,value in [('igesVolume',24),('igesArea',51),('igesColor',False),('stlTriangles',11),('sourceOwnersDisposed',False)]:
        values=copy.deepcopy(base);values['format-chain.json'][field]=value;save(values)
        try:verify_resource_reports(root,fixture)
        except AssertionError:checks+=1
        else:raise AssertionError('Invalid chain evidence accepted')
print(f'Owned resource evidence fixtures PASS: 1 positive/{checks} negative cases')
