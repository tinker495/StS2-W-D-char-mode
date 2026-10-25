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

## Spec Review

- Treat explicit user requirements and corrections as spec evidence, even when no separate spec file exists. Read the original issue or design document for context, and carry the user's newer settled decisions into the review when older documents disagree.
- When reviewing named base-game effects, use the relevant [glossary](../../GLOSSARY.md) entries and apply [ADR-0001](../adr/0001-reuse-base-game-effects.md) to the effect name, original type, and described behavior. Include source evidence identifying the original type, member, version, and behavior; check for contradictions within the requirements as well as between the requirements and implementation.
- Include these sources and settled decisions in any delegated Spec review so the reviewer can distinguish an unresolved conflict from a choice the user has already made.
