# Output Format

Use este formato para relatorios persistidos de cobertura:

```markdown
# Coverage Analysis - <ProjectName>

| Metric | Value |
|--------|-------|
| Date | <YYYY-MM-DD> |
| Line Coverage | <N>% |
| Branch Coverage | <N>% |
| Risk Hotspots | <N> |
| Tests | <N> passed, <N> failed |

## Summary

<Resumo curto do resultado e dos riscos principais.>

## Risk Hotspots

| Rank | Method | Class | File | Complexity | Coverage | CRAP Score |
|------|--------|-------|------|------------|----------|------------|
| 1 | `<method>` | `<class>` | `<file>` | <N> | <N>% | <score> |

## Coverage Gaps by File

| File | Line Coverage | Branch Coverage | Uncovered Lines | Priority |
|------|---------------|-----------------|-----------------|----------|
| `<file>` | <N>% | <N>% | <N> | HIGH/MED/LOW |

## Recommendations

1. <Acao recomendada e motivo.>

## Reports

| Report | Path |
|--------|------|
| Markdown summary | `<coverageDir>/coverage-analysis.md` |
| Raw Cobertura XML | `<path>` |
```
