"""Validate exact CE resource evidence independently from the test outcome flag."""
import hashlib,json
from pathlib import Path

def verify_resource_reports(directory,fixture,module=False):
    root=Path(directory);fixture=Path(fixture)
    read=lambda p:json.loads(p.read_text(encoding='utf-8-sig'))
    identity=read(fixture/'fixture-identity.json')
    inputs={x['path']:x['sha256'] for x in identity['inputs']}
    for path,digest in inputs.items():assert hashlib.sha256((fixture/path).read_bytes()).hexdigest().upper()==digest
    rows=[]
    expected=[('嵌套.gltf','Gltf',{'嵌套.gltf','parts/网格.bin','images/色块.png'}),('嵌入.gltf','Gltf',{'嵌入.gltf','parts/嵌入.bin'}),('嵌套.obj','Obj',{'嵌套.obj','materials/材料.mtl','materials/textures/色块.png'})]
    for main,kind,names in expected:
        row=read(root/(Path(main).stem+'-'+kind+'.json'))
        assert row['state']=='PASS' and row['main']==main and row['format']==(0 if kind=='Gltf' else 1)
        for field in ('sourceDeleted','resultDisposed','documentAliasSurvived','shapeSurvived'):assert row[field] is True,field
        texture=row['texture'];assert texture['Width']==texture['Height']==2
        assert texture['Slot']==(0 if kind=='Obj' else 1)
        assert texture['EncodedPng']==list((fixture/'images/色块.png').read_bytes())
        assert row['hash']==identity['pngSha256']==hashlib.sha256(bytes(texture['EncodedPng'])).hexdigest().upper()
        corners=texture['Corners'];assert len(corners)==4
        for n,pixel in enumerate(corners):
            assert (pixel['X'],pixel['Y'])==(n&1,(n>>1)&1)
            assert [pixel[x] for x in ('Red','Green','Blue')]==identity['corners'][n] and pixel['Alpha']==1
        resources=row['resources'];assert len(resources)==len(names) and {x['FileName'] for x in resources}==names
        for entry in resources:assert entry['Sha256']==inputs[entry['FileName']] and entry['Bytes']==(fixture/entry['FileName']).stat().st_size
        rows.append(row)
    chain=read(root/'format-chain.json')
    assert chain['state']=='PASS' and chain['sourceVolume']==24 and chain['igesFaces']==6 and chain['stlTriangles']==12
    assert chain['sourceDeleted'] is True and chain['sourceOwnersDisposed'] is True
    if module:assert chain['igesVolume']==24 and chain['profile']=='geometry-only BRep-mode IGES; no facade metadata API'
    else:
        assert chain['igesVolume']==0 and chain['igesArea']==52
        assert chain['igesSolidTopology']=='lost by existing default surface-mode writer'
        assert all(chain[x] is True for x in ('igesName','igesColor','igesLayer'))
        assert chain['report']['SourceEntityCount']>0 and chain['report']['TransferredRootCount']==1
        assert chain['report']['SourceLengthUnitMeters']==.001 and chain['report']['SystemLengthUnitMillimeters']==1
        assert chain['stlMetadata']=='not carried by STL format'
    return rows,chain
