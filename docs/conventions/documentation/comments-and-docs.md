# Comments and Markdown Documentation

Code comments and tracked Markdown explain **why** a non-obvious decision, setting, constraint, or workaround exists. They do not restate **what** the code or configuration already expresses, and they never accumulate consecutive work logs.

Git history, PR descriptions, and GitHub issues own change chronology. Tracked source files and Markdown describe the current system and the reasoning behind its non-obvious choices.

## Code Comments

### Keep

- **Why behind non-obvious behavior:** rationale for a setting, threshold, ordering contract, or invariant that types and identifiers cannot convey.
- **Engine and runtime constraints:** StS2, BaseLib, Godot, or Spine quirks explaining why a workaround or defensive pattern is required.
- **Non-obvious math and coordinate contracts:** reference frames, units, or sign conventions when an expression alone is ambiguous.
- **Required test provenance:** requester and context comments mandated by [test ownership](/docs/agents/test-ownership.md) on formal tests and validators.

### Remove

- **What-comments:** narration that repeats visible code flow, section banners restating method or field names, and XML/doc comments that merely echo identifiers, parameters, or return types.
- **Work-history comments:** sequential edit logs, pass or iteration notes (`pass 2`, `v3`), dated amendment trails, `"previously X / changed to Y / removed Z"` explanations, debugging chronicles, and PR/issue recap notes.
- **Dead code:** commented-out code blocks and stale resolved notes.

## Markdown Documentation

### Keep

- **Self-contained contracts and procedures:** active specifications, settled gameplay/art rules, runtime resource mappings, and reproducible procedures in their owning documents, written as the current state. Record applied behavior and assets using production code, runtime resource paths, or verified game deployments.
- **Decision rationale ("Why"):** concise explanation of why an adopted design, setting, or art direction was chosen, what constraints shaped it, and what trade-offs it accepts (for example in `/docs/adr/` and `<char>-history.md`).
- **Evidence and verification scope:** state the scope of any game verification in its owning record (such as [deployment status](/docs/deployment-status.md)), replacing superseded runs rather than appending a running journal. Identify original-game or external evidence through public sources or versions, types, and members.

### Remove

- **Consecutive work logs:** chronological narratives (`first we tried X, then fixed Y, then revised Z`), dated session/review diaries, PR integration logs, completed implementation plans, and before/after tweak histories.
- **What-restatements:** prose that mirrors implementation details already readable in source code, manifests, or scenes, or duplicates facts owned by another document instead of linking to it.
- **Unapplied work artifacts:** links, embeds, or records of specific drafts, candidate assets, comparison renders, one-off runs, local cache outputs, or superseded plans never applied to the game; keep this evidence in the task report or PR when needed.

### Repository Links

- Use repository-root-relative destinations (such as `/docs/agents/workflow.md`) whenever a link would otherwise traverse a parent directory; resolve a leading `/` against the Git repository root when opening files.
- Use local paths (including `./`) for same-folder and descendant links, `#section` for same-document anchors, and the owning source's path rules for external plugin resources.
- Keep Markdown repository links free of `..` path segments.

## Protected Boundaries

- Never modify `tests/` or protected validation/CI assets under [test ownership](/docs/agents/test-ownership.md) during documentation or comment cleanup; report findings in protected files separately for human review.
- Keep third-party or upstream-vendored documentation untouched unless explicitly included in the task.
