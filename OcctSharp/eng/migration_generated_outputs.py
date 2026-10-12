"""Preserve every delivered generated output except exact admitted source changes."""
def verify_generated_output_hashes(baseline, current, affected):
    if not baseline.keys() <= current.keys():
        raise ValueError('Delivered generated output removed.')
    rows = list(affected)
    allowed = {row['path']: row for row in rows}
    if len(allowed) != len(rows) or not allowed.keys() <= baseline.keys():
        raise ValueError('Affected output identities are duplicate or outside the delivered baseline.')
    unchanged, modified = [], []
    for path, old_sha in baseline.items():
        new_sha = current[path]
        if path in allowed:
            row = allowed[path]
            if row['baselineSha256'] != old_sha or not row['reason'].strip() or old_sha == new_sha:
                raise ValueError('Affected output lacks exact baseline, rationale or real change: ' + path)
            if 'sha256' in row and row['sha256'] != new_sha:
                raise ValueError('Audited changed output drifted: ' + path)
            modified.append(dict(path=path,baselineSha256=old_sha,sha256=new_sha,reason=row['reason']))
        else:
            if new_sha != old_sha:
                raise ValueError('Unaudited generated output changed: ' + path)
            unchanged.append(dict(path=path,sha256=new_sha))
    return unchanged, modified
