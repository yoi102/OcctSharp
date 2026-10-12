"""Negative and positive fixtures for binding transitions versus new parser discovery."""
import unittest
from migration_delivery_delta import verify_inventory_delta


class DeltaTests(unittest.TestCase):
    def test_explicit_manual_conversion_is_separate_from_new_bindings(self):
        verify_inventory_delta({'a':'Manual','b':'Blocked'}, {'a':'Emitted','b':'Emitted'}, {'a','b'}, set(), 2, 0, manual_to_emitted={'a'})

    def test_manual_conversion_without_exact_authority_fails(self):
        with self.assertRaises(ValueError):
            verify_inventory_delta({'a':'Manual'}, {'a':'Emitted'}, {'a'}, set(), 1, 0)

    def test_conversion_cannot_relabel_blocked_or_unchanged_manual(self):
        for old,new in [('Blocked','Emitted'),('Manual','Manual')]:
            with self.assertRaises(ValueError):
                verify_inventory_delta({'a':old}, {'a':new}, {'a'}, set(), 1, 0, manual_to_emitted={'a'})

    def test_conversion_cannot_hide_outside_generated_delta(self):
        with self.assertRaises(ValueError):
            verify_inventory_delta({'a':'Manual'}, {'a':'Manual'}, set(), set(), 0, 0, manual_to_emitted={'a'})

    def test_unchanged_identities_and_exact_transition(self):
        verify_inventory_delta({'a':'Blocked','b':'Manual'}, {'a':'Emitted','b':'Manual'}, {'a'}, set(), 1, 0)

    def test_discovery_is_explicit_and_separate_from_migration(self):
        verify_inventory_delta({'a':'Blocked'}, {'a':'Emitted','b':'Blocked','c':'Skipped'}, {'a'}, set(), 1, 0,
                               {'b':'Blocked','c':'Skipped'})

    def test_unrecorded_new_identity_fails(self):
        with self.assertRaises(ValueError):
            verify_inventory_delta({'a':'Blocked'}, {'a':'Emitted','b':'Blocked'}, {'a'}, set(), 1, 0)

    def test_new_discovery_cannot_claim_implementation(self):
        with self.assertRaises(ValueError):
            verify_inventory_delta({'a':'Blocked'}, {'a':'Emitted','b':'Emitted'}, {'a'}, set(), 1, 0, {'b':'Emitted'})

    def test_missing_prior_identity_fails(self):
        with self.assertRaises(ValueError):
            verify_inventory_delta({'a':'Blocked','b':'Skipped'}, {'a':'Emitted'}, {'a'}, set(), 1, 0)

    def test_changed_discovery_state_fails(self):
        with self.assertRaises(ValueError):
            verify_inventory_delta({'a':'Blocked'}, {'a':'Emitted','b':'Skipped'}, {'a'}, set(), 1, 0, {'b':'Blocked'})

    def test_unaudited_old_transition_fails(self):
        with self.assertRaises(ValueError):
            verify_inventory_delta({'a':'Blocked','b':'Blocked'}, {'a':'Emitted','b':'Manual'}, {'a'}, set(), 1, 0)


if __name__ == '__main__':
    unittest.main()
