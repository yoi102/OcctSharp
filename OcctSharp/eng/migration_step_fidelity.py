"""Fail-closed verification of configured STEP fixture fields and independent file identity."""
import hashlib, math

def close(a,b):
    if type(a) is not type(b) and not (type(a) in (int,float) and type(b) in (int,float)):return False
    if type(a) in (int,float):return math.isfinite(a) and math.isfinite(b) and abs(a-b)<1e-6
    if isinstance(a,list):return len(a)==len(b) and all(close(x,y) for x,y in zip(a,b))
    if isinstance(a,dict):return a.keys()==b.keys() and all(close(a[k],b[k]) for k in a)
    return a==b

def verify_step_fidelity(report,ledger,step_bytes):
    profile=report.get('Schema','')+'-'+report.get('Domain','')
    if profile not in ledger['profiles']:raise ValueError('Unknown STEP schema/document profile.')
    if report.get('SourceDeleted') is not True or report.get('SourceOwnerDisposed') is not True or report.get('FreeRoots')!=1:raise ValueError('Missing independent STEP lifetime/root evidence.')
    expected_volume=24 if report['Domain']=='Native' else 6000
    if not close(report.get('Volume'),expected_volume):raise ValueError('Independent STEP geometry differs.')
    if not isinstance(step_bytes,bytes) or hashlib.sha256(step_bytes).hexdigest().upper()!=report.get('Sha256'):raise ValueError('Actual STEP file hash differs.')
    text=step_bytes.decode('utf-8')
    schema={'Ap203':'CONFIG_CONTROL_DESIGN','Ap214':'AUTOMOTIVE_DESIGN','Ap242':'AP242'}[report['Schema']]
    if 'FILE_SCHEMA' not in text or schema not in text:raise ValueError('Actual STEP schema differs.')
    expected=ledger['profiles'][profile];fields=report.get('Fields',[]);seen=set()
    for row in fields:
        key=row.get('Group','')+'.'+row.get('Name','')
        if key in seen or key not in expected or row.get('Fidelity')!=expected[key]:raise ValueError('STEP field catalogue/boundary differs.')
        seen.add(key)
        a,b=row['Expected'],row['Observed'];state=row['Fidelity']
        if state=='supported-exact' and (a!=b or type(a) is not type(b)):raise ValueError('Claimed exact field differs.')
        if state=='supported-numeric-tolerance' and not close(a,b):raise ValueError('Claimed numerical fidelity differs.')
        if state=='lost-or-changed' and close(a,b):raise ValueError('Claimed loss/approximation is not observed.')
        if state not in ('supported-exact','supported-numeric-tolerance','lost-or-changed'):raise ValueError('Unrecognized STEP field disposition.')
    if seen!=set(expected):raise ValueError('STEP field evidence is incomplete.')

def verify_step_profiles(directory,ledger,profiles):
    import json
    for profile in profiles:
        report=json.loads((directory/(profile+'.json')).read_text(encoding='utf-8-sig'))
        verify_step_fidelity(report,ledger,(directory/(profile+'.step')).read_bytes())
