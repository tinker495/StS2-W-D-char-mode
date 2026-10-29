# Topic-Based Character Guides

Use this layout for every character and other visual owner, including Ancients. Resolve the actual owner through [naming and layout](../characters/naming.md); `<char>` is a parameter, never a hardcoded Realta path. Named characters use their approved storage name, and undecided characters keep their existing placeholder name.

## Entry Points and Ownership

| Location under `docs/characters/<char>/` | Owns |
|---|---|
| `<char>.md` or an existing linked design document | Human-facing gameplay, setting and settled product decisions |
| `<char>-art.md` | Short index routing an art request to the relevant specification |
| `<char>-assets.md` | Short index routing delivery status, filenames and integration references |
| `visuals/<topic>/art.md`, `appearance.md` or another focused specification | That topic's approved appearance, reference roles, constraints and prompt material |
| `visuals/<topic>/assets.md` | Current draft/approved/applied state, resource mapping, dimensions and delivery procedure |
| `visuals/<topic>/api.md` | Native/BaseLib integration evidence, consulted when code wiring changes |
| `visuals/<topic>/inventory*.md` | Large filename tables split by a useful selector such as rarity; linked from a small index |
| `<char>-history.md` | Past choices, rejected directions and observations |
| `base.png`, `refs/`, optional ignored `drafts/` | Canonical pixels, supplied references, and disposable production records |

Choose topics by the work: `base`, `ui`, `animation`, `selection`, `cards`, `relics`, `powers`, or an owner's own topic such as an Ancient `event`. Use `shared` only for material needed by several topics. Create only topics with actual content; this is not a mandatory directory scaffold. A small cohesive page can own both a short mapping table and its API notes.

## Read and Update One Topic

Start from the relevant art/asset index and follow only the requested topic. Load identity and palette only when visible character features need them. Consult API evidence when changing integration; select only the inventory relevant to the target. Realta's file names and number of topics are examples, not dependencies for Althea, Aselige or future owners.

After a delivery, update its specification and asset page. Keep the entry indexes short rather than appending detailed delivery logs. Link to the shared [deployment record](../../deployment-status.md) for package/install/game verification. Preserve distinctions between a candidate, approval, runtime application and installation.

When moving existing sections, preserve decisions, resource mappings, source versions and code identifiers. Rewrite relative links, incoming anchors and references such as "section 2" or "below" to their new owners. Keep canonical images and runtime resources at their existing paths unless a separate migration is authorized. Reuse the existing repository reference checks; document reorganization does not add tests or change acceptance criteria.

## Owners Without Art Guides

An absent guide or base does not make the routing convention inapplicable. Inspect the owner's current model, resource overrides and settled user requirements first. If the requested work needs a new specification, create the relevant topic and its short index using observed facts; mark missing references and unresolved appearance explicitly. Do not invent a base, an approval or a whole set of empty topics.

For the current undecided `CharacterFour` model, use `character_four` while that name remains active. Preserve its existing placeholder behavior; a legacy constant such as CharacterFour's `CharacterId` is not an approved identity specification. An Ancient uses its own scene/model APIs, while a folder containing vanilla reference material is not automatically a mod-character implementation.
