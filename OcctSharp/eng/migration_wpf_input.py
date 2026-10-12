"""Independently verify real WPF input evidence and decoded desktop pixel changes."""
import json,hashlib
from pathlib import Path
from PIL import Image

def verify_wpf_input(directory,native_directory,reference=None):
    root=Path(directory);native=Path(native_directory)
    read=lambda p:json.loads(p.read_text(encoding='utf-8-sig'))
    result=read(root/'input-workflow.json');repro=read(root/'capture-loss-reproduction.json')
    assert result['state']==repro['state']=='PASS' and repro['staleClick'] is False and repro['errors']==0
    for field in ('identity','captureLoss','rectangleCancel','freshRectangle','nativeEscape','nativeCancelMode','cameraRejected','removedRejected','isolation'):
        assert result[field] is True,field
    hwnd=result['actualWpfChildHwnd'];assert isinstance(hwnd,int) and hwnd!=0
    assert repro['originalCapture']==hwnd and repro['transferredCapture'] not in (hwnd,0)
    events=result['events'];assert events and all(e['hwnd']==hwnd for e in events)
    for message in (0x201,0x202,0x200,0x207,0x208,0x204,0x205,0x20a,0x100,0x1f):assert any(e['message']==message for e in events)
    def pixels(name):
        with Image.open(root/(name+'.png')) as image:
            assert image.size==(260,260) and image.format=='PNG'
            data=list(image.convert('RGB').getdata())
        assert sum(sum(p)>30 for p in data)>500,'Blank desktop evidence'
        return data
    frames={name:pixels(name) for name in ('normal','zoom','pan','rotate','before-clip','clipped','restored','other-before','other-after')}
    assert frames['other-before']==frames['other-after'] and result['otherPixelsChanged']==0
    if reference is not None:
        for name,data in frames.items():
            with Image.open(Path(reference)/(name+'.png')) as image:
                assert image.size==(260,260) and data==list(image.convert('RGB').getdata()),('Actual desktop model differs from inspected reference',name)
    for old,new,field in [('normal','zoom','zoomPixels'),('zoom','pan','panPixels'),('pan','rotate','rotationPixels'),('before-clip','clipped','clipPixels'),('clipped','restored','clipRestorePixels')]:
        changed=sum(a!=b for a,b in zip(frames[old],frames[new]));assert changed==result[field] and changed>50,(field,changed)
    modules=read(root/'loaded-native.json');assert len(modules)==62 and len({Path(x['path']).name for x in modules})==62
    for row in modules:
        source=Path(row['path']);assert hashlib.sha256(source.read_bytes()).hexdigest().upper()==row['sha256']
        assert hashlib.sha256((native/source.name).read_bytes()).hexdigest().upper()==row['sha256']
    return result
