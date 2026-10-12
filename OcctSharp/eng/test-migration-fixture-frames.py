import struct,unittest,zlib
from migration_fixture_frames import model_pixels,verify_fixture_frame

def png(raw=None,header=None):
    raw=raw if raw is not None else bytes(1081*360)
    def chunk(name,data):return struct.pack('>I',len(data))+name+data+struct.pack('>I',zlib.crc32(name+data))
    return b'\x89PNG\r\n\x1a\n'+chunk(b'IHDR',header or struct.pack('>IIBBBBB',360,360,8,2,0,0,0))+chunk(b'IDAT',zlib.compress(raw))+chunk(b'IEND',b'')

class Frames(unittest.TestCase):
    def test_complete_model_region_and_selection_change_rejection(self):
        a=png();raw=bytearray(1081*360);raw[180*1081+1+90*3]=255
        self.assertEqual(len(model_pixels(a)),328*328*3)
        verify_fixture_frame(a,a)
        with self.assertRaises(ValueError):verify_fixture_frame(png(raw),a)
    def test_desktop_corner_is_outside_model_region(self):
        raw=bytearray(1081*360);raw[359*1081+1]=255
        verify_fixture_frame(png(raw),png())
    def test_reject_malformed_crc_filter_format_and_expansion(self):
        a=png();bad=bytearray(a);bad[24]^=1
        raw=bytearray(1081*360);raw[0]=1
        for b in (bytes(bad),a[:-1],a+b'x',png(raw),png(bytes(1081*361)),png(bytes(1081*359)),png(header=struct.pack('>IIBBBBB',360,360,8,6,0,0,0))):
            with self.subTest(length=len(b)),self.assertRaises(ValueError):model_pixels(b)

if __name__=='__main__':unittest.main()
