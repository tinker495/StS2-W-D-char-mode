# Topic-Based Character Guides

Use this layout for every character and other visual owner, including Ancients. Resolve the actual owner through [naming and layout](/docs/conventions/characters/naming.md); `<char>` is a parameter, never a hardcoded Realta path. Named characters use their approved storage name, and undecided characters keep their existing placeholder name.

## Entry Points and Ownership

| Location under `docs/characters/<char>/` | Owns |
|---|---|
| `<char>.md` or an existing linked design document | Human-facing gameplay, setting and settled product decisions |
| `<char>-art.md` | Short index routing an art request to the relevant specification |
| `<char>-assets.md` | Short index routing delivery status, filenames and integration references |
| `<topic>-art.md`, `<topic>-appearance.md` or another focused specification | That topic's approved appearance, reference roles, constraints and prompt material |
| `<topic>-assets.md` | Applied assets, runtime resource mapping, dimensions, and delivery procedure |
| `<topic>-<subject>.md#appearance` and `#delivery-and-integration` | The same responsibilities in separate sections of one short, cohesive subject document |
| `<topic>-api.md` | Native/BaseLib integration evidence, consulted when code wiring changes |
| `<topic>-inventory*.md` | Large filename tables split by a useful selector such as rarity, linked from a small index |
| `animation/` or another executable topic directory | Authoring scripts, manifests and tools reached through descendant links from the topic documents |
| `<char>-history.md` | Rationale for adopted art decisions |
| `base.png`, `refs/`, optional ignored `drafts/` | Non-duplicate high-resolution canonical master (omit when runtime holds verbatim pixels), character identity references plus pending topic roughs, and disposable local records |

Choose topics according to the work, using `base`, `ui`, `animation`, `selection`, `cards`, `relics`, `powers`, or an owner's own topic such as an Ancient `event`. Use `shared` only for material needed by several topics. Put topic documents directly in the character directory and prefix their filenames with the topic, such as `ui-energy.md` or `cards-inventory-uncommon.md`. Create documents only for actual content. Same-folder documents link by filename. Links to `refs/` and executable inputs use descendant paths. A workflow document whose location is required by a protected validator stays at that location and is linked from the topic documents.

### Post-Application Asset Lifecycle

- **Verbatim runtime masters:** Keep `docs/characters/<char>/base.png` only when the canonical master differs from shipped runtime pixels (higher resolution or rig source). When an owner's approved painting is applied verbatim to `Sts2WD/` (for example an Ancient event painting), omit `docs/characters/<char>/base.png` and link art/asset documents directly to `/Sts2WD/...`.
- **Character refs vs. topic roughs:** Character-level identity files directly under `docs/characters/<char>/refs/` (`refs/ref*`) remain tracked. Asset-specific roughs inside `docs/characters/<char>/refs/<topic>/` are tracked only while pending application: remove each rough as soon as its runtime texture ships in `Sts2WD/`, record required motif constraints in `<topic>-art.md`, and remove the `refs/<topic>/` directory (including any index Markdown) once zero pending non-Markdown roughs remain.
- **Staging outputs:** Keep candidates, comparison boards, and overview sheets exclusively in Git-ignored `drafts/` or `artifacts/`, never as tracked files under `docs/characters/`.

When a subject's short appearance and delivery records repeat the same decisions or require reading together, keep them in one subject document with `Appearance` and `Delivery and Integration` sections. Point the art and asset indexes directly to the corresponding sections. A short mapping table and its integration notes can stay there too. Keep large inventories and integration references separate when only some tasks need them.

## Read and Update One Topic

Start from the relevant art/asset index and follow only the requested topic. Load identity and palette only when visible character features need them. Consult API evidence when changing integration; select only the inventory relevant to the target. Realta's file names and number of topics are examples, not dependencies for Althea, Aselige or future owners.

After applying artwork, update the topic's appearance specification and runtime mapping. Keep the entry indexes short. Link to the shared [deployment record](/docs/deployment-status.md) for recorded game installations, and identify the verification actually performed.

When moving existing sections, preserve decisions, resource mappings, source versions and code identifiers. Rewrite relative links, incoming anchors and references such as "section 2" or "below" to their new owners. Keep canonical images and runtime resources at their existing paths unless a separate migration is authorized. Reuse the existing repository reference checks; document reorganization does not add tests or change acceptance criteria.

## Owners Without Art Guides

An absent guide or base does not make the routing convention inapplicable. Inspect the owner's current model, resource overrides and settled user requirements first. If the requested work needs a new specification, create the relevant topic and its short index using observed facts; mark missing references and unresolved appearance explicitly. Do not invent a base, an approval or a whole set of empty topics.

For any future undecided `CharacterN` model, use `character_n` while that placeholder name remains active. Preserve its existing placeholder behavior; a legacy `CharacterId` constant alone is not an approved identity specification. An Ancient uses its own scene/model APIs, while a folder containing vanilla reference material is not automatically a mod-character implementation.
