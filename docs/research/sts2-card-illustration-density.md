# StS2 Card Illustration Density

Agent reference for selecting card-art anchors and reviewing thumbnail density. Observed on 2026-10-28 after the user rejected a Realta card candidate as too detailed and requested a broad native-card comparison.

## Source and scope

Primary evidence: the locally installed `SlayTheSpire2.pck`; associated `sts2.dll` ProductVersion `0.1.0+59260271157f76a2896f0eab5bc6ea1245d8b314`. This identifies the inspected installation, not a claimed public patch label. MegaDot `v4.5.1.m.14` exported the native portrait textures. The sample covers 40 non-beta portraits across five character pools. It is a purposeful visual sample, not a census or a gameplay-type audit.

Native source layout: `res://images/packed/card_portraits/<pool>/<card_id>.png`. Each identifier below completes that path. There are 39 landscape `1000 × 760` portraits and one vertical `606 × 852` portrait, `silent/corpse_explosion`. The landscape portraits were opened at `250 × 190`; the vertical portrait was observed separately as a composition example and does not establish equal-area standard-card density.

| Pool | Inspected card IDs |
|---|---|
| `ironclad` | `bludgeon`, `body_slam`, `uppercut`, `whirlwind`, `tear_asunder`, `sword_boomerang`, `perfected_strike`, `stampede`, `infernal_blade`, `fiend_fire` |
| `silent` | `predator`, `assassinate`, `precise_cut`, `echoing_slash`, `dagger_spray`, `dash`, `grand_finale`, `corpse_explosion` |
| `necrobinder` | `severance`, `reap`, `the_scythe`, `sculpting_strike`, `bone_shards`, `flatten`, `end_of_days`, `times_up` |
| `regent` | `conqueror`, `devastate`, `photon_cut`, `heavenly_drill`, `kingly_punch`, `collision_course`, `big_bang`, `falling_star` |
| `defect` | `sunder`, `hyperbeam`, `meteor_strike`, `glasswork`, `shatter`, `sweeping_beam` |

To reproduce, mount the installed pack with Godot's `ProjectSettings.load_resource_pack`, load the listed PNG paths as `Texture2D`, and export each `get_image()` with `save_png()`. Use a temporary minimal host project and an explicit writable `--log-file`. Run the observation through the repository's [temporary-observation workflow](../development-tools.md). Use the packed portrait paths above: extracting an `AtlasTexture` can fail when its atlas image remains compressed. Preserve complete portraits and inspect them at equal displayed illustration sizes. Keep extracted game artwork and comparison sheets in ignored temporary storage; the names, source layout, build identity, and observations here are sufficient without those local files.

## Observed visual patterns

| Question | Native examples and observations | Production implication |
|---|---|---|
| Character plus targets | `echoing_slash` groups crossed foreground arms below several severed targets; one broad arc joins them. `whirlwind` groups a rear-view figure inside a dominant sweep. | Select multi-subject examples when the brief includes an attacker and a target. A weapon-only close-up does not establish the right framing or figure density. |
| Aftermath | `grand_finale` retains a complete central figure and defeated creatures, but pushes the surrounding masses into darker, quieter colors. | A planted finishing pose can work. Establish hierarchy through grouped values rather than replacing the user's pose with a generic close-up strike. |
| Clear bisection | `photon_cut` separates two recognizable object sections around a bright cut; `severance` uses a spare horizontal gap against a quiet field; `precise_cut` gives the crossing beam a decisive silhouette. | Preserve recognizable halves and clear negative space. Additional fragments are optional supporting shapes, not the explanation of the cut. |
| Transparent material | `glasswork` uses a few large curved translucent planes, a broad pale rim, and selective interior value changes. `times_up` makes an hourglass readable through outline, fill, and a concentrated warm accent. | Glass can retain volume without dozens of bubbles, thin reflections, and chips. It need not become a uniformly faceted polygon object. |
| Dense focal detail | `bone_shards`, `tear_asunder`, and `heavenly_drill` contain many edges around the hand, rupture, or tool while much of the backdrop stays broad. | Detail is not forbidden. Allocate it to the card's focal action; avoid equally detailed costume, target, effects, floor, and architecture. |
| Large effects | `hyperbeam`, `meteor_strike`, and `falling_star` use dominant coherent trajectories and supporting fragments. `sweeping_beam` connects multiple subjects through one broad effect. | Read the effect as a connected action first. Supporting particles should reinforce that action rather than form a second luminous field. |
| Setting | `end_of_days` reduces a city to a graphic silhouette; `grand_finale` subdues its surrounding scene; `dash` suggests a floor through broad directional planes. | Establish a location with a few useful masses. Individually lit paving stones and repeated masonry contours need a scene-specific reason. |
| Rendering range | The sample includes hard color boundaries, softer transitions, selective outlines, varied numbers of value planes, and detailed focal shapes. | There is no single universal two-tone or fixed-fragment recipe. Match the subject family and hierarchy of several relevant cards. |

These are observations of the listed artwork and project judgments derived from them, not published Mega Crit production rules or automatic acceptance criteria.

## Anchor selection and review

Survey a broad set before choosing production inputs. Group candidates by visual problem: actor/target composition, action/result, material, and background hierarchy. Select a small complementary set from the survey and assign each input a role. Passing every surveyed image into one generation call can mix conflicting compositions and obscure the intended reference.

For Realta's rear-view Aeonglass bisection, the closest working set is `whirlwind` (rear-view figure/action hierarchy), `photon_cut` (recognizable separated sections), `grand_finale` (aftermath hierarchy), and `glasswork` (economical transparency). `echoing_slash` supplies an additional actor/target grouping example but uses foreground arms rather than a rear-view body. Preserve Realta's own composition and identity. These sources supply visual principles, not replacement actors, weapons, poses, or palette.

Compare complete native illustration textures with the candidate at `250 × 190`. A whole card fitted into that rectangle makes its artwork much smaller and cannot support a fair density judgment. Examine where secondary edges and highlights attract attention, not only whether the subjects remain identifiable. Retain selective form shading when removing incidental detail; simplifying everything to equal polygon facets can introduce a different mismatch.

## Session retrospective

1. **Review did not drive correction:** the initial notes recognized excess debris and floor detail but treated it as an optional later revision because the small image remained readable. The user had to trigger the correction. The [pipeline's default loop](../../.agents/skills/character-asset-pipeline/SKILL.md#default-generatereviewcorrect-loop) now connects pixel observations to bounded, in-scope corrections and rechecks preserved features before a final recommendation.
2. **Equipment depth was underspecified:** intact visible broom/blade ends were treated as sufficient evidence of a continuous weapon, without tracing the shaft through the body overlap and hand. The later request exposed that gap. Anatomical front/back and camera-near/far must be distinguished; local edit prompts now specify occluders and the object's continuous route. The user subsequently limited this turn to pipeline changes, so no revised shaft image was generated or verified.
3. **Reference selection:** a single `Thrumming Hatchet` close-up was too narrow for a rear-view character confronting a giant target, and a whole-card comparison made the native artwork smaller than the candidate. The broader survey and equal-size portrait comparison address those distinct errors. Broad investigation and the smaller set of actual generation inputs are recorded separately.
4. **Prompt priorities:** the initial prompt requested glass shards, droplets, chips, paving, ruined architecture, reflected light, and extensive equipment construction while also asking for economy. The concrete additions dominated the general simplification instruction. Compact card recipes now describe the action, visible identity cues, and allocation of detail; images supply construction. Local corrections use a narrow edit recipe instead of restarting the full brief.
5. **Tool constraints:** a generation request exceeded the tool's five-local-image input limit. The invocation guidance now requires input-role selection within the active limit before calling the tool. A local overlap correction need not consume the full new-generation reference set.
6. **Tool economy:** repeated broad file reads produced truncated outputs despite an existing bounded-read workflow. Use heading lookup followed by focused ranges and reuse observations of unchanged inputs; no additional always-loaded rule is needed.

The repository already has a pre-commit build/reference check. It does not judge visual hierarchy. No new tests, validators, or automated density thresholds were introduced for this art correction.
