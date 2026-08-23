# ADR-0037: Keep initial IGES read transfer native-local

- Status: Accepted
- Date: 2026-08-22

## Context

IGES reader transfer roots and transient model state are native-owned. The first
data-exchange extension needs a symmetric read path without exposing reader state.

## Decision

Expose `ShapeExchange.ReadIges`, which validates a path, runs
`IGESControl_Reader.ReadFile` and `TransferRoots` in the bridge, and returns one
owning `Shape`. File, transfer, and null-result failures use the existing status
and diagnostic contract.

## Consequences

This closes the initial IGES geometry round trip but does not claim generated IGES
declaration coverage, reader parameters, or metadata/XDE transfer.

## Validation

Release/Debug runtime tests and the clean package consumer must verify writing an
IGES box, reading it back, and source-disposal independence.
