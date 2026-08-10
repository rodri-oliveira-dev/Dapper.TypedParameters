Last completed prompt: 010
Current status: Completed
Branch: feat/table-valued-parameters
Last expected commit: feat: add table-valued parameter support
Next prompt: 011-package-quality

## Handoff notes

- Remote update over SSH failed with `Permission denied (publickey)`.
- Local `main` was fast-forwarded from `feat/output-parameters` because remote
  update over SSH was not available in this environment.
- Prompt 010 adds explicit `DataTable` TVP support and leaves
  `SqlDataRecord` overloads for a future decision.
