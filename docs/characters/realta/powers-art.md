# Realta Power Icon Art

- The 19 named symbols in [refs/powers](refs/powers/README.md) define the primary motifs for those powers. Preserve each specified symbol and arrangement while applying shared game rendering rather than reinterpreting the card name; these are raw references, not runtime artwork.
- Two motifs carry explicit production overrides over the raw sheets: 요정 장인 uses green leaf-like wings around a rectangular craft hammer (no central orb, concentric socket, or circular wing spots that suggest an eye); 강철 포식자 depicts a visibly broken sword and separated steel shards against a straight, thick, intact wall slab (no curved, bent, cracked, or yielding wall).
- 불멸의 충성 reuses 원초의 형상 via an exact horizontal reflection at both runtime sizes (`64×64` and `256×256`), preserving colors, transparency, dimensions, and the complete canvas without redrawing.
- **Two files per power:** `Sts2WD/images/powers/big/realta_<name>_power.png` at 256×256 RGBA; `Sts2WD/images/powers/realta_<name>_power.png` at 64×64, resized.
- Use a **single persistent-state emblem**, not a miniature card scene: two or three bold shapes and two or three primary colors.
- `Amount` overlays the bottom-right `x/y:60–100%`. Put identifying tips/eyes/clock hands/motifs central-to-upper-left, `x/y:15–80%`.
- Distinguish related powers by contour (shield/anvil/crossed weapons/wing/ring) and dominant color.
