"""Inspect the exact RGB PNG format emitted by BatchAUVisibleWindow."""
import struct,zlib

def rgb(path):
 data=path.read_bytes();assert data[:8]==b'\x89PNG\r\n\x1a\n'
 offset=8;compressed=bytearray();header=None
 while offset<len(data):
  size=struct.unpack_from('>I',data,offset)[0];kind=data[offset+4:offset+8]
  body=data[offset+8:offset+8+size];crc=struct.unpack_from('>I',data,offset+8+size)[0]
  assert len(body)==size and zlib.crc32(kind+body)==crc
  if kind==b'IHDR':header=struct.unpack('>IIBBBBB',body)
  if kind==b'IDAT':compressed.extend(body)
  offset+=12+size
 assert offset==len(data) and header==(360,360,8,2,0,0,0)
 raw=zlib.decompress(compressed);assert len(raw)==360*1081
 assert all(raw[y*1081]==0 for y in range(360))
 return b''.join(raw[y*1081+1:(y+1)*1081] for y in range(360))

def interior_counts(before,after):
 assert len(before)==len(after)==360*360*3
 positions=[(y*360+x)*3 for y in range(16,344) for x in range(16,344)]
 def foreground(frame):
  corner=frame[(16*360+16)*3:(16*360+16)*3+3]
  return sum(any(abs(frame[i+c]-corner[c])>8 for c in range(3)) for i in positions)
 return foreground(before),foreground(after),sum(before[i:i+3]!=after[i:i+3] for i in positions)
