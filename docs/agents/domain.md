# Domain Documentation

This repository uses one domain context: the root `GLOSSARY.md` for terminology and `docs/adr/` for design decisions.

## Before Exploring Code

- Read `GLOSSARY.md` if it exists.
- Read ADRs in `docs/adr/` that concern the current work.
- Continue if these documents are absent. Do not report their absence separately or propose empty documents. The `domain-modeling` skill creates the necessary documents when terminology or design decisions are established.

## Terminology and Decisions

- Use `GLOSSARY.md` terms when referring to domain concepts in issue titles, refactoring proposals, hypotheses, and similar material.
- If a needed term is missing, first check whether the project actually uses it. Address a genuine gap through `domain-modeling`.
- When a proposal conflicts with an existing ADR, identify that ADR and explain the conflict.
