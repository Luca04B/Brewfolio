# Domain docs

Brewfolio uses a single domain-documentation context.

## Before exploring

- Read `CONTEXT.md` before naming or changing domain concepts.
- Read relevant decisions under `docs/adr/` before revisiting them.
- Continue silently when a referenced document does not exist.

## Layout

```text
/
├── CONTEXT.md
├── docs/
│   └── adr/
└── src/
```

## Vocabulary

Use domain terms exactly as defined in `CONTEXT.md`. Avoid synonyms that the glossary explicitly rejects.

When a required concept is missing, reconsider whether existing vocabulary covers it. Record genuine terminology gaps through the domain-modeling workflow.

## Architecture decisions

Surface conflicts with existing ADRs explicitly instead of silently overriding them.
