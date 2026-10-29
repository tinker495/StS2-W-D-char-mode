# Althea Selection Delivery

## Background Conversion

The selected illustration master is 1672×941 RGB. Whole-image Fit into 1920×1080 preserves the complete painting, with the approximately 1px Fit margin and resampling edge backed by opaque navy `#101321`; no image pixels are cropped. The runtime selection `TextureRect` uses `KEEP_ASPECT_CENTERED` (`stretch_mode = 5`), not Cover. The sheathed sword extends toward the lower-right illustration boundary; no post-generation crop is performed.

## Selection Revision — Approved and Applied

The user selected high-angle candidate C and the new fully sheathed pre-draw background after the UI overlap was reported. The tile preserves a concentrated upward gaze under lowered brows. The background shows a lowered left-facing head, hilt hand, bracing hand and curse currents gathering at the scabbard throat. It replaces the earlier drawn-sword painting. The normal/locked pair shares the same complete-image Fit alpha; the background uses whole-image Fit with an opaque navy backing. Current runtime files, not ignored drafts, are the shared visual source of truth.

Full-image previews and a 132×195 comparison with native Silent/Defect tiles were opened. An approximate selection UI composite keeps the face and both hands clear; the ascension panel overlaps the bottom of the crossguard. This known composition tradeoff was disclosed before the user requested application. It is not a live-game observation. The selected composition is preserved in the runtime textures in the [UI inventory](../ui/assets.md).

Portrait, marker, energy, shared standing and scene wiring remain as previously approved. The base hash remains unchanged. Runtime selection textures are 132×195, 132×195 and 1920×1080, all opaque. Earlier publish attempts reported MSBuild Exec -1 after savepack and succeeded outside the sandbox. The logs contain `EXEC : error : Failed to read the root certificate store.`; this was output parsing, not evidence of an engine crash. After separating that environmental diagnostic from real errors and requiring savepack completion, one sandbox publish completed successfully without escalation or retry. A separate clean engine mounted the resulting PCK, loaded all nine Althea textures and four scenes, and observed the selection dimensions and Fit mode 5; it exited 0. Local game installation and live gameplay verification were not performed. No tests or acceptance assets were added or changed.
