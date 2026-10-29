# Reuse Native StS2 Features

Prefer StS2's existing features and extension points for gameplay, commands, models, UI, tooltips, localization, and resource handling. Before adding a mod-owned equivalent or a Harmony patch, inspect the relevant native type, member, and vanilla call site in the installed game. Use the native API directly or through established BaseLib integration points; keep content and presentation changes within that native flow.

Add custom behavior only for a required capability the native implementation lacks or a confirmed defect. Record the game version, relevant type/member, observed limitation, and why existing APIs or extension points are insufficient in the implementation documentation. Keep the extension limited to that gap and preserve native behavior outside it. For named base-game effects and their mappings, follow [ADR-0001](../../adr/0001-reuse-base-game-effects.md).
