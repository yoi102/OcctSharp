"""Validate independent generated and private Manual migration transitions."""
def verify_inventory_delta(old, new, generated, manual, generated_count, manual_count, discovered=None, manual_to_emitted=None):
    generated, manual = set(generated), set(manual)
    discovered = {} if discovered is None else dict(discovered)
    converted = set() if manual_to_emitted is None else set(manual_to_emitted)
    if not converted <= generated:
        raise ValueError('Manual to Emitted identities must be explicitly included in the generated delta.')
    if not old.keys() <= new.keys():
        raise ValueError('Previously inventoried identities were removed.')
    if new.keys() - old.keys() != discovered.keys():
        raise ValueError('New discovery identities differ from the audited parser delta.')
    if any(new[sid] != state or state not in ('Blocked', 'Skipped') for sid, state in discovered.items()):
        raise ValueError('New discovery disposition differs or silently claims implementation.')
    changed = {sid for sid in old if old[sid] != new[sid]}
    if generated & manual or changed != generated | manual:
        raise ValueError('Audited generated/Manual sets do not partition the delta.')
    if len(generated) != generated_count or len(manual) != manual_count:
        raise ValueError('Generated or Manual delta count differs.')
    if any(old[sid] != ('Manual' if sid in converted else 'Blocked') or new[sid] != 'Emitted' for sid in generated):
        raise ValueError('Generated transitions must be Blocked to Emitted or explicitly audited Manual to Emitted.')
    if any(old[sid] != 'Blocked' or new[sid] != 'Manual' for sid in manual):
        raise ValueError('Manual transitions must be Blocked to Manual.')
