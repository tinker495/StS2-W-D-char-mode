# Architecture Review Criteria

Apply these judgement-based criteria to shared code and every character. Mechanical validation belongs to the existing checks.

- Go beyond minimal edits to diff lines. When a change reveals structural problems such as duplicated ownership of a concept, shotgun surgery, delegation-only wrappers, or eroding stage boundaries, propose deeper alternatives explicitly: moving, merging, or deleting modules, or reassigning ownership.
- Optimize for minimum context rather than minimum change. Prefer deep modules with narrow interfaces that reduce the concepts maintainers must understand. Favor removing complexity over adding code.
- Flag speculative generality as a deletion candidate: single-implementation interfaces, configuration nobody sets, and hand-rolled replacements for standard-library functionality.
- Support structural recommendations with evidence of their actual maintenance cost and present them in severity order.
