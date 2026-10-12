import unittest
import importlib.util
from pathlib import Path

spec = importlib.util.spec_from_file_location('development_version', Path(__file__).with_name('set-development-version.py'))
module = importlib.util.module_from_spec(spec)
spec.loader.exec_module(module)


class VersionTests(unittest.TestCase):
    def test_identity_and_exact_acceptance_move_together(self):
        source = '8.0.1-preview.55 ABI 1.98 bridge 0.106.0 new Version(1, 98) 0x00010062U'
        self.assertEqual('8.0.1-preview.56 ABI 1.99 bridge 0.107.0 new Version(1, 99) 0x00010063U',
                         module.replace_identity(source, '8.0.1-preview.55', '8.0.1-preview.56', '1.98', '1.99', '0.106.0', '0.107.0'))

    def test_other_sdk_and_dependency_versions_remain(self):
        source = 'OCCT 8.0.1 oneTBB 2022.0 unrelated 0.8.0'
        self.assertEqual(source, module.replace_identity(source, '8.0.1-preview.55', '8.0.1-preview.56', '1.98', '1.99', '0.106.0', '0.107.0'))

    def test_invalid_encoded_components_are_rejected(self):
        with self.assertRaises(ValueError):
            module.replace_identity('', '8.0.1-preview.55', '8.0.1-preview.56', '1.98', '1.65536', '0.106.0', '0.107.0')

    def test_partial_versions_are_not_changed(self):
        source = '11.98 1.980 0.106.00 8.0.1-preview.550'
        self.assertEqual(source, module.replace_identity(source, '8.0.1-preview.55', '8.0.1-preview.56', '1.98', '1.99', '0.106.0', '0.107.0'))


if __name__ == '__main__':
    unittest.main()
