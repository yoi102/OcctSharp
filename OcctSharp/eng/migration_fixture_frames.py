"""Decode the bounded RGB/filter-zero PNG format written by the actual HWND fixture."""
import struct,zlib

def model_pixels(png):
    if len(png)>2*1024*1024 or png[:8]!=b'\x89PNG\r\n\x1a\n':
        raise ValueError('Invalid fixture PNG')
    offset=8;data=[];header=None;ended=False
    while offset<len(png):
        if offset+12>len(png):raise ValueError('Truncated PNG chunk')
        count=struct.unpack('>I',png[offset:offset+4])[0];kind=png[offset+4:offset+8]
        end=offset+8+count
        if end+4>len(png):raise ValueError('Truncated PNG payload')
        payload=png[offset+8:end]
        if zlib.crc32(kind+payload)!=struct.unpack('>I',png[end:end+4])[0]:raise ValueError('PNG CRC differs')
        if kind==b'IHDR':
            if header is not None:raise ValueError('Duplicate PNG header')
            header=payload
        elif kind==b'IDAT':data.append(payload)
        elif kind==b'IEND':
            if payload or end+4!=len(png):raise ValueError('Invalid PNG ending')
            ended=True;break
        offset=end+4
    if not ended or header!=struct.pack('>IIBBBBB',360,360,8,2,0,0,0):
        raise ValueError('Fixture PNG dimensions/format differ')
    stride=360*3+1;expected=stride*360
    decoder=zlib.decompressobj();raw=decoder.decompress(b''.join(data),expected+1)
    if len(raw)!=expected or not decoder.eof or decoder.unconsumed_tail or decoder.unused_data:
        raise ValueError('Fixture PNG decompression budget/length differs')
    if any(raw[y*stride]!=0 for y in range(360)):raise ValueError('Unexpected fixture row filter')
    # The complete rendered model/selection fits in [64,296); this larger ROI
    # retains surrounding background. Desktop pixels at rounded window corners
    # depend on the underlying desktop and do not belong to the model region.
    return b''.join(raw[y*stride+1+16*3:y*stride+1+344*3] for y in range(16,344))

def verify_fixture_frame(actual,reference):
    if model_pixels(actual)!=model_pixels(reference):
        raise ValueError('Actual model/selection pixels differ from the inspected fixture')
