Last completed prompt: 009
Current status: Completed
Branch: feat/output-parameters
Last expected commit: feat: add output parameter support
Next prompt: 010-table-valued-parameters

## Handoff notes

- Remote update over SSH failed with `Permission denied (publickey)`.
- HTTPS showed `origin/main` at `308c68e`, which does not include the local
  prompt 008 commit.
- The local `feat/string-parameters` branch contains prompt 006 and 007 work
  that was not present in the accessible `main`; the prompt 009 branch combines
  that local phase-2 state with prompt 008 before implementing outputs.
